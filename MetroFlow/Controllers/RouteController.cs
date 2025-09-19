using MetroFlow.Models;
using MetroFlow.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MetroFlow.Controllers
{
    public class RouteController : Controller
    {
        private readonly ILogger<RouteController> _logger;
        private readonly ILocationService _locationService;
        private readonly IHeatmapService _heatmapService;
        private readonly IScheduleService _scheduleService;
        private readonly IBillingService _billingService;

        public RouteController(ILogger<RouteController> logger, ILocationService locationService, IHeatmapService heatmapService, IScheduleService scheduleService, IBillingService billingService)
        {
            _logger = logger;
            _locationService = locationService;
            _heatmapService = heatmapService;
            _scheduleService = scheduleService;
            _billingService = billingService;
        }

        [HttpGet]
        public IActionResult RoutePlanner(string userLat = null, string userLng = null, string destination = null, bool autoCalculate = false, bool showLocationPrompt = false)
        {
            var stations = _locationService.GetAllStations();
            var model = GetModelFromSession() ?? new RoutePlannerViewModel();
            model.AllStations = stations;

            // Handle incoming parameters from popular stations
            if (!string.IsNullOrEmpty(userLat) && !string.IsNullOrEmpty(userLng))
            {
                if (double.TryParse(userLat, out double lat) && double.TryParse(userLng, out double lng))
                {
                    model.CurrentLatitude = lat;
                    model.CurrentLongitude = lng;
                    model.CurrentLocationText = "Current Location Detected";
                    model.NearestCurrentStation = _locationService.FindNearestStation(lat, lng);

                    // Clear any previously selected origin when using current location
                    model.SelectedOrigin = null;
                    model.OriginQuery = string.Empty;
                    model.OriginSuggestions.Clear();
                    model.SuccessMessage = "Current location set from popular station selection!";
                }
            }

            // Handle destination parameter (station name from popular stations)
            if (!string.IsNullOrEmpty(destination))
            {
                var destinationStation = MapDestinationToStation(destination);
                if (destinationStation != null)
                {
                    model.SelectedDestination = new Place
                    {
                        Name = destinationStation.Name,
                        DisplayName = destinationStation.Name,
                        Latitude = destinationStation.Latitude,
                        Longitude = destinationStation.Longitude
                    };
                    model.DestinationQuery = destinationStation.Name;
                    model.NearestDestinationStation = destinationStation;
                    model.DestinationSuggestions.Clear();

                    if (string.IsNullOrEmpty(model.SuccessMessage))
                    {
                        model.SuccessMessage = $"Destination set to {destinationStation.Name}!";
                    }
                    else
                    {
                        model.SuccessMessage += $" Destination set to {destinationStation.Name}!";
                    }

                    // Auto-calculate route if we have both origin and destination
                    if (autoCalculate && (model.HasCurrentLocation || model.HasOrigin) && model.HasDestination)
                    {
                        try
                        {
                            SaveModelToSession(model);
                            return PlanTrip(); // This will calculate and return the route
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error auto-calculating route from popular station");
                            model.ErrorMessage = "Route calculated with some limitations. Please verify the details.";
                        }
                    }
                }
                else
                {
                    model.ErrorMessage = "Selected station not found. Please try selecting a different station.";
                }
            }

            if (showLocationPrompt)
            {
                model.ErrorMessage = "Location access was denied. Please manually set your origin or enable location services.";
            }

            // Clear any previous route info if we're just setting up
            if (!autoCalculate)
            {
                model.RouteInfo = null;
                model.FareBilling = null;
            }

            SaveModelToSession(model);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SetCurrentLocation(double latitude, double longitude)
        {
            try
            {
                var model = GetModelFromSession() ?? new RoutePlannerViewModel();
                model.AllStations = _locationService.GetAllStations();
                model.CurrentLatitude = latitude;
                model.CurrentLongitude = longitude;
                model.CurrentLocationText = "Current Location Detected";
                model.NearestCurrentStation = _locationService.FindNearestStation(latitude, longitude);

                // Clear any selected origin when using current location
                model.SelectedOrigin = null;
                model.OriginQuery = string.Empty;
                model.OriginSuggestions.Clear();
                model.SuccessMessage = "Current location set successfully!";
                model.ErrorMessage = null;

                // Clear any previous route info
                model.RouteInfo = null;
                model.FareBilling = null;

                SaveModelToSession(model);
                return View("RoutePlanner", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting current location");
                var model = GetModelFromSession() ?? new RoutePlannerViewModel();
                model.AllStations = _locationService.GetAllStations();
                model.ErrorMessage = "Unable to set current location. Please try again.";
                model.SuccessMessage = null;
                return View("RoutePlanner", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SearchDestination(string destinationQuery)
        {
            var model = GetModelFromSession() ?? new RoutePlannerViewModel();
            model.AllStations = _locationService.GetAllStations();
            model.DestinationQuery = destinationQuery ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(destinationQuery) && destinationQuery.Length >= 2)
            {
                try
                {
                    model.DestinationSuggestions = await _locationService.SearchPlacesAsync(destinationQuery);
                    if (!model.DestinationSuggestions.Any())
                    {
                        model.ErrorMessage = "No places found. Try a different search term.";
                        model.SuccessMessage = null;
                    }
                    else
                    {
                        model.ErrorMessage = null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error searching for destination: {Query}", destinationQuery);
                    model.ErrorMessage = "Search temporarily unavailable. Please try again.";
                    model.SuccessMessage = null;
                }
            }
            else
            {
                model.DestinationSuggestions.Clear();
            }

            SaveModelToSession(model);
            return View("RoutePlanner", model);
        }

        [HttpPost]
        public async Task<IActionResult> SearchOrigin(string originQuery)
        {
            var model = GetModelFromSession() ?? new RoutePlannerViewModel();
            model.AllStations = _locationService.GetAllStations();
            model.OriginQuery = originQuery ?? string.Empty;

            // Clear current location when searching for origin
            model.CurrentLatitude = null;
            model.CurrentLongitude = null;
            model.CurrentLocationText = "Get Current Location";
            model.NearestCurrentStation = null;

            if (!string.IsNullOrWhiteSpace(originQuery) && originQuery.Length >= 2)
            {
                try
                {
                    model.OriginSuggestions = await _locationService.SearchPlacesAsync(originQuery);
                    if (!model.OriginSuggestions.Any())
                    {
                        model.ErrorMessage = "No places found. Try a different search term.";
                        model.SuccessMessage = null;
                    }
                    else
                    {
                        model.ErrorMessage = null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error searching for origin: {Query}", originQuery);
                    model.ErrorMessage = "Search temporarily unavailable. Please try again.";
                    model.SuccessMessage = null;
                }
            }
            else
            {
                model.OriginSuggestions.Clear();
            }

            SaveModelToSession(model);
            return View("RoutePlanner", model);
        }

        [HttpPost]
        public IActionResult SelectDestination(string name, string displayName, double latitude, double longitude)
        {
            try
            {
                var model = GetModelFromSession() ?? new RoutePlannerViewModel();
                model.AllStations = _locationService.GetAllStations();
                model.SelectedDestination = new Place
                {
                    Name = name,
                    DisplayName = displayName,
                    Latitude = latitude,
                    Longitude = longitude
                };
                model.DestinationQuery = displayName;
                model.DestinationSuggestions.Clear();
                model.NearestDestinationStation = _locationService.FindNearestStation(latitude, longitude);
                model.SuccessMessage = "Destination selected successfully!";
                model.ErrorMessage = null;
                model.RouteInfo = null;
                model.FareBilling = null;

                SaveModelToSession(model);
                return View("RoutePlanner", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting destination");
                var model = GetModelFromSession() ?? new RoutePlannerViewModel();
                model.AllStations = _locationService.GetAllStations();
                model.ErrorMessage = "Unable to select destination. Please try again.";
                model.SuccessMessage = null;
                return View("RoutePlanner", model);
            }
        }

        [HttpPost]
        public IActionResult SelectOrigin(string name, string displayName, double latitude, double longitude)
        {
            try
            {
                var model = GetModelFromSession() ?? new RoutePlannerViewModel();
                model.AllStations = _locationService.GetAllStations();
                model.SelectedOrigin = new Place
                {
                    Name = name,
                    DisplayName = displayName,
                    Latitude = latitude,
                    Longitude = longitude
                };
                model.OriginQuery = displayName;
                model.OriginSuggestions.Clear();
                model.NearestOriginStation = _locationService.FindNearestStation(latitude, longitude);
                model.SuccessMessage = "Origin selected successfully!";
                model.ErrorMessage = null;
                model.RouteInfo = null;
                model.FareBilling = null;

                SaveModelToSession(model);
                return View("RoutePlanner", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting origin");
                var model = GetModelFromSession() ?? new RoutePlannerViewModel();
                model.AllStations = _locationService.GetAllStations();
                model.ErrorMessage = "Unable to select origin. Please try again.";
                model.SuccessMessage = null;
                return View("RoutePlanner", model);
            }
        }

        [HttpPost]
        public IActionResult PlanTrip()
        {
            try
            {
                var model = GetModelFromSession() ?? new RoutePlannerViewModel();
                model.AllStations = _locationService.GetAllStations();

                Station originStation = null;
                double originLatitude = 0, originLongitude = 0, walkingDistanceToOrigin = 0;

                if (model.HasOrigin)
                {
                    originLatitude = model.SelectedOrigin!.Latitude;
                    originLongitude = model.SelectedOrigin.Longitude;
                    if (model.NearestOriginStation == null)
                    {
                        model.NearestOriginStation = _locationService.FindNearestStation(originLatitude, originLongitude);
                    }
                    originStation = model.NearestOriginStation;
                    walkingDistanceToOrigin = _locationService.CalculateDistance(
                        originLatitude, originLongitude,
                        originStation.Latitude, originStation.Longitude);
                }
                else if (model.HasCurrentLocation)
                {
                    originLatitude = model.CurrentLatitude!.Value;
                    originLongitude = model.CurrentLongitude!.Value;
                    if (model.NearestCurrentStation == null)
                    {
                        model.NearestCurrentStation = _locationService.FindNearestStation(originLatitude, originLongitude);
                    }
                    originStation = model.NearestCurrentStation;
                    walkingDistanceToOrigin = _locationService.CalculateDistance(
                        originLatitude, originLongitude,
                        originStation.Latitude, originStation.Longitude);
                }
                else
                {
                    model.ErrorMessage = "Please set your origin location first.";
                    model.SuccessMessage = null;
                    return View("RoutePlanner", model);
                }

                if (!model.HasDestination)
                {
                    model.ErrorMessage = "Please select a destination first.";
                    model.SuccessMessage = null;
                    return View("RoutePlanner", model);
                }

                if (model.NearestDestinationStation == null)
                {
                    model.NearestDestinationStation = _locationService.FindNearestStation(
                        model.SelectedDestination!.Latitude, model.SelectedDestination.Longitude);
                }

                if (originStation!.Name == model.NearestDestinationStation!.Name)
                {
                    model.ErrorMessage = "You are already near your destination station!";
                    model.SuccessMessage = null;
                    return View("RoutePlanner", model);
                }

                double walkingDistanceFromDest = _locationService.CalculateDistance(
                    model.SelectedDestination!.Latitude, model.SelectedDestination.Longitude,
                    model.NearestDestinationStation.Latitude, model.NearestDestinationStation.Longitude);

                model.RouteInfo = _locationService.CalculateRoute(
                    originStation,
                    model.NearestDestinationStation,
                    walkingDistanceToOrigin,
                    walkingDistanceFromDest
                );

                // FIX: Ensure route stations are in the correct travel direction
                if (model.RouteInfo?.RouteStations != null && model.RouteInfo.RouteStations.Any())
                {
                    // Get station indices to determine travel direction
                    var allStations = _locationService.GetAllStations();
                    var originIndex = allStations.FindIndex(s => s.Name == originStation.Name);
                    var destIndex = allStations.FindIndex(s => s.Name == model.NearestDestinationStation.Name);

                    // If traveling from south to north (higher index to lower index), reverse the route
                    if (originIndex > destIndex)
                    {
                        model.RouteInfo.RouteStations.Reverse();
                    }

                    // Ensure the route starts with the origin station (additional safety check)
                    if (model.RouteInfo.RouteStations.First().Name != originStation.Name)
                    {
                        model.RouteInfo.RouteStations.Reverse();
                    }
                }

                // Calculate billing information
                if (model.RouteInfo?.OriginStation != null && model.RouteInfo?.DestinationStation != null)
                {
                    model.FareBilling = _billingService.CalculateFare(
                        model.RouteInfo.OriginStation,
                        model.RouteInfo.DestinationStation,
                        model.RouteInfo.Stations
                    );
                }

                model.SuccessMessage = "Route calculated successfully!";
                model.ErrorMessage = null;

                SaveModelToSession(model);
                return View("RoutePlanner", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error planning trip");
                var model = GetModelFromSession() ?? new RoutePlannerViewModel();
                model.AllStations = _locationService.GetAllStations();
                model.ErrorMessage = "Unable to plan trip. Please try again.";
                model.SuccessMessage = null;
                return View("RoutePlanner", model);
            }
        }

        [HttpPost]
        public IActionResult ClearRoute()
        {
            HttpContext.Session.Remove("RoutePlannerModel");
            var model = new RoutePlannerViewModel
            {
                AllStations = _locationService.GetAllStations()
            };
            return View("RoutePlanner", model);
        }

        [HttpGet]
        public IActionResult GetBillingInfo(string originStation, string destinationStation)
        {
            try
            {
                if (string.IsNullOrEmpty(originStation) || string.IsNullOrEmpty(destinationStation))
                {
                    return Json(new { success = false, error = "Origin and destination stations are required" });
                }

                var origin = _locationService.FindStationByName(originStation);
                var destination = _locationService.FindStationByName(destinationStation);

                if (origin == null || destination == null)
                {
                    return Json(new { success = false, error = "Station not found" });
                }

                var stationsCount = Math.Abs(origin.Id - destination.Id);
                var billing = _billingService.CalculateFare(origin, destination, stationsCount);

                return Json(new { success = true, billing = billing });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Error calculating fare: " + ex.Message });
            }
        }

        // AJAX search endpoints
        [HttpGet]
        public async Task<IActionResult> SearchDestinationAjax(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Json(new List<Place>());

            try
            {
                var suggestions = await _locationService.SearchPlacesAsync(query);
                return Json(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AJAX destination search: {Query}", query);
                return Json(new List<Place>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchOriginAjax(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Json(new List<Place>());

            try
            {
                var suggestions = await _locationService.SearchPlacesAsync(query);
                return Json(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AJAX origin search: {Query}", query);
                return Json(new List<Place>());
            }
        }

        // Original heatmap endpoint (keep for backward compatibility)
        [HttpGet]
        public IActionResult GetHeatmapData()
        {
            try
            {
                var currentPeriod = GetCurrentServicePeriod();
                var heatmapData = _heatmapService.GetAllStationsHeatmapData(currentPeriod);
                return Json(new
                {
                    success = true,
                    currentPeriod = currentPeriod,
                    stations = heatmapData,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting heatmap data");
                return Json(new { success = false, error = "Unable to load heatmap data" });
            }
        }

        // New heatmap endpoint with time period selection
        [HttpGet]
        public IActionResult GetHeatmapDataByPeriod(string timePeriod = "current")
        {
            try
            {
                string currentPeriod;
                if (timePeriod == "current")
                {
                    currentPeriod = GetCurrentServicePeriod();
                }
                else
                {
                    // Extract day type from time period
                    string dayType = timePeriod.StartsWith("weekday") ? "Weekday" : "Friday";
                    currentPeriod = _heatmapService.GetPeriodTypeFromTimeRange(timePeriod, dayType);
                }

                var heatmapData = _heatmapService.GetAllStationsHeatmapData(currentPeriod);
                var timePeriodOptions = _heatmapService.GetTimePeriodOptions();

                return Json(new
                {
                    success = true,
                    currentPeriod = currentPeriod,
                    selectedTimePeriod = timePeriod,
                    stations = heatmapData,
                    timePeriodOptions = timePeriodOptions,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting heatmap data for period: {TimePeriod}", timePeriod);
                return Json(new { success = false, error = "Unable to load heatmap data" });
            }
        }

        [HttpGet]
        public IActionResult HeatmapAnalysis(string originStation, string destinationStation)
        {
            if (string.IsNullOrEmpty(originStation) || string.IsNullOrEmpty(destinationStation))
            {
                return RedirectToAction("RoutePlanner");
            }

            var model = new HeatmapAnalysisViewModel
            {
                OriginStationName = originStation,
                DestinationStationName = destinationStation
            };

            return View(model);
        }

        [HttpGet]
        [HttpGet]
        public IActionResult GetPopularStations()
        {
            try
            {
                var popularStations = _locationService.GetAllStations()
                    .OrderByDescending(s => s.PopularityIndex)
                    .Take(2)
                    .Select(s => new
                    {
                        Id = GetStationId(s.Name), // Use the helper method
                        Name = s.Name,
                        PopularityIndex = s.PopularityIndex,
                        Description = GetStationDescription(s.Name),
                        ImageUrl = GetStationImageUrl(s.Name)
                    })
                    .ToList();

                return Json(new { success = true, stations = popularStations });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular stations");
                return Json(new { success = false, error = "Unable to load popular stations" });
            }
        }


        // Helper method to get station descriptions
        // Helper method to get station descriptions
        private string GetStationDescription(string stationName)
        {
            var descriptions = new Dictionary<string, string>
    {
        { "Agargaon", "Government administrative center with easy access to ministries, secretariat buildings, and official institutions" },
        { "Bijoy Sarani", "Strategic commercial location connecting major roads with shopping centers, offices, and city landmarks" },
        { "Dhaka University", "Academic hub station serving the prestigious University of Dhaka campus and surrounding educational facilities" },
        { "Farmgate", "Central business district station providing access to offices, shopping centers, restaurants, and educational institutions" },
        { "Karwan Bazar", "Bustling commercial area famous for wholesale markets, shopping complexes, and vibrant business activities" },
        { "Kazipara", "Residential and commercial area serving local communities with markets, schools, and convenient metro connectivity" },
        { "Mirpur 10", "Major residential and commercial hub connecting thousands of daily commuters to shopping malls and business centers" },
        { "Mirpur 11", "Densely populated residential station serving the Mirpur-11 community with local markets and amenities" },
        { "Motijheel", "Prime commercial and financial district with major banks, corporate headquarters, and government offices" },
        { "Pallabi", "Large residential area with local markets, community facilities, and easy access to surrounding neighborhoods" },
        { "Secretariat", "Government headquarters station providing direct access to Bangladesh Secretariat and administrative offices" },
        { "Shahbagh", "Cultural and educational hub near museums, galleries, academic institutions, and historical landmarks" },
        { "Shewrapara", "Local residential station providing metro access to Shewrapara neighborhood with community facilities" },
        { "Uttara Center", "Central Uttara location with modern shopping malls, restaurants, residential complexes, and business centers" },
        { "Uttara North", "Northern residential area in planned Uttara with modern infrastructure, schools, and commercial facilities" },
        { "Uttara South", "Southern Uttara station connecting residential communities to the main city with convenient transport links" }
    };

            return descriptions.ContainsKey(stationName)
                ? descriptions[stationName]
                : "Major metro station providing excellent connectivity and access to local attractions and facilities";
        }


        // Helper method to get station image URLs
        // Helper method to get station image URLs
        private string GetStationImageUrl(string stationName)
        {
            var imageUrls = new Dictionary<string, string>
    {
        { "Agargaon", "https://upload.wikimedia.org/wikipedia/commons/thumb/3/39/Agargaon_IDB_Railway_Station.jpg/1200px-Agargaon_IDB_Railway_Station.jpg" },
        { "Bijoy Sarani", "https://www.tbsnews.net/sites/default/files/styles/infograph/public/images/2023/bijoy-sarani-metro-station.jpg" },
        { "Dhaka University", "https://media.prothomalo.com/prothomalo-english%2F2024-12-08%2Fio2fkf4l%2Fdu-metro-station.jpg?rect=0%2C288%2C1280%2C672&w=1200&ar=40%3A21&auto=format%2Ccompress&ogImage=true&mode=crop&overlay=&overlay_position=bottom&overlay_width_pct=1" },
        { "Farmgate", "https://upload.wikimedia.org/wikipedia/commons/0/04/Farmgate_metro_station_3.jpg" },
        { "Karwan Bazar", "https://tds-images.thedailystar.net/sites/default/files/styles/very_big_201/public/images/2023/12/31/karwan_bazar.jpg" },
        { "Kazipara", "https://images.daily-bangladesh.com/media/imgAll/2023December/kazipara-metro-station.jpg" },
        { "Mirpur 10", "https://d2u0ktu8omkpf6.cloudfront.net/4c32ec48c8a8041a57a32a3d5a078782486a2b921b52ca56.jpg" },
        { "Mirpur 11", "https://images.prothomalo.com/prothomalo-bangla%2F2023-12%2F2f8b9c4d-1a2e-4f5b-8e9f-3c4d5e6f7a8b%2Fmirpur11-metro.jpg" },
        { "Motijheel", "https://cdn.bdnews24.com/bdnews24/media/bdnews24-english/2023-11/10421368-b099-4b50-93c2-c7d6df6825a9/metro_rail_motijheel_051123_11.jpg" },
        { "Pallabi", "https://www.dhakatribune.com/media/2023/12/28/pallabi-metro-station.jpg" },
        { "Secretariat", "https://upload.wikimedia.org/wikipedia/commons/thumb/8/8e/Segunbagicha_%28Bangladesh_Secretariat%29_metro_station.jpg/500px-Segunbagicha_%28Bangladesh_Secretariat%29_metro_station.jpg" },
        { "Shahbagh", "https://upload.wikimedia.org/wikipedia/commons/thumb/9/9e/Shahbag_metro_station_building.jpg/500px-Shahbag_metro_station_building.jpg" },
        { "Shewrapara", "https://www.newagebd.net/files/records/news/202312/190833_184.jpg" },
        { "Uttara Center", "https://rapidpass.com.bd/storage/galleries/1748061479-29-1.jpg" },
        { "Uttara North", "https://rapidpass.com.bd/storage/galleries/1748061479-29-1.jpg" },
        { "Uttara South", "https://rapidpass.com.bd/storage/galleries/1748061479-29-1.jpg" }
    };

            return imageUrls.ContainsKey(stationName)
                ? imageUrls[stationName]
                : "https://rapidpass.com.bd/storage/galleries/1748061479-29-1.jpg"; // Default metro station image
        }

        // Helper method to convert station names to URL-friendly IDs
        private string GetStationId(string stationName)
        {
            return stationName.ToLower()
                .Replace(" ", "-")
                .Replace("'", "")
                .Replace(".", "")
                .Trim();
        }


        // Helper method to map station identifiers to actual stations
        // Helper method to map station identifiers to actual stations
        // Helper method to map station identifiers to actual stations
        // Helper method to map station identifiers to actual stations
        private Station? MapDestinationToStation(string destinationId)
        {
            var allStations = _locationService.GetAllStations();

            // Complete mapping for all 16 stations based on your database
            var stationMap = new Dictionary<string, string>
    {
        // Main stations (exact matches from DB)
        { "agargaon", "Agargaon" },
        { "bijoy-sarani", "Bijoy Sarani" },
        { "dhaka-university", "Dhaka University" },
        { "farmgate", "Farmgate" },
        { "karwan-bazar", "Karwan Bazar" },
        { "kazipara", "Kazipara" },
        { "mirpur-10", "Mirpur 10" },
        { "mirpur-11", "Mirpur 11" },
        { "motijheel", "Motijheel" },
        { "pallabi", "Pallabi" },
        { "secretariat", "Secretariat" },
        { "shahbagh", "Shahbagh" },
        { "shewrapara", "Shewrapara" },
        { "uttara-center", "Uttara Center" },
        { "uttara-north", "Uttara North" },
        { "uttara-south", "Uttara South" },
        
        // Alternative formats/names people might use
        { "mirpur10", "Mirpur 10" },
        { "mirpur-10-station", "Mirpur 10" },
        { "mirpur11", "Mirpur 11" },
        { "mirpur-11-station", "Mirpur 11" },
        { "du", "Dhaka University" },
        { "dhaka-uni", "Dhaka University" },
        { "university", "Dhaka University" },
        { "farm-gate", "Farmgate" },
        { "karwan", "Karwan Bazar" },
        { "karwanbazar", "Karwan Bazar" },
        { "moti-jheel", "Motijheel" },
        { "motijhil", "Motijheel" },
        { "uttara-centre", "Uttara Center" },
        { "uttara-1", "Uttara North" },
        { "uttara-3", "Uttara South" }
    };

            if (stationMap.ContainsKey(destinationId.ToLower()))
            {
                var stationName = stationMap[destinationId.ToLower()];
                return allStations.FirstOrDefault(s => s.Name.Equals(stationName, StringComparison.OrdinalIgnoreCase));
            }

            // Fallback: try direct name match
            return allStations.FirstOrDefault(s =>
                s.Name.Equals(destinationId, StringComparison.OrdinalIgnoreCase) ||
                s.Name.Replace(" ", "-").ToLower().Equals(destinationId.ToLower()) ||
                s.Name.Replace(" ", "").ToLower().Equals(destinationId.ToLower()));
        }




        private string GetCurrentServicePeriod()
        {
            try
            {
                return _scheduleService.GetCurrentPeriodType();
            }
            catch
            {
                return "Normal Hour";
            }
        }

        // Session Helpers
        private void SaveModelToSession(RoutePlannerViewModel model)
        {
            try
            {
                var serializedModel = JsonSerializer.Serialize(model);
                HttpContext.Session.SetString("RoutePlannerModel", serializedModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving model to session");
                SetTempDataFallback(model);
            }
        }

        private RoutePlannerViewModel? GetModelFromSession()
        {
            try
            {
                var serializedModel = HttpContext.Session.GetString("RoutePlannerModel");
                if (string.IsNullOrEmpty(serializedModel))
                {
                    return GetModelFromTempDataFallback();
                }

                var model = JsonSerializer.Deserialize<RoutePlannerViewModel>(serializedModel);
                model.AllStations = _locationService.GetAllStations();
                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving model from session");
                return GetModelFromTempDataFallback();
            }
        }

        // Fallback storage in TempData
        private void SetTempDataFallback(RoutePlannerViewModel model)
        {
            TempData["CurrentLatitude"] = model.CurrentLatitude?.ToString();
            TempData["CurrentLongitude"] = model.CurrentLongitude?.ToString();
            TempData["CurrentLocationText"] = model.CurrentLocationText;
            TempData["NearestCurrentStationName"] = model.NearestCurrentStation?.Name;
            TempData["NearestCurrentStationDistance"] = model.NearestCurrentStation?.Distance?.ToString();

            // Destination
            TempData["DestinationQuery"] = model.DestinationQuery;
            TempData["SelectedDestinationName"] = model.SelectedDestination?.Name;
            TempData["SelectedDestinationDisplayName"] = model.SelectedDestination?.DisplayName;
            TempData["SelectedDestinationLatitude"] = model.SelectedDestination?.Latitude.ToString();
            TempData["SelectedDestinationLongitude"] = model.SelectedDestination?.Longitude.ToString();
            TempData["NearestDestinationStationName"] = model.NearestDestinationStation?.Name;
            TempData["NearestDestinationStationDistance"] = model.NearestDestinationStation?.Distance?.ToString();

            // Origin
            TempData["OriginQuery"] = model.OriginQuery;
            TempData["SelectedOriginName"] = model.SelectedOrigin?.Name;
            TempData["SelectedOriginDisplayName"] = model.SelectedOrigin?.DisplayName;
            TempData["SelectedOriginLatitude"] = model.SelectedOrigin?.Latitude.ToString();
            TempData["SelectedOriginLongitude"] = model.SelectedOrigin?.Longitude.ToString();
            TempData["NearestOriginStationName"] = model.NearestOriginStation?.Name;
            TempData["NearestOriginStationDistance"] = model.NearestOriginStation?.Distance?.ToString();

            // Route info
            if (model.RouteInfo != null)
            {
                TempData["RouteInfoStations"] = model.RouteInfo.Stations.ToString();
                TempData["RouteInfoMetroTime"] = model.RouteInfo.MetroTime.ToString();
                TempData["RouteInfoWalkingTime"] = model.RouteInfo.WalkingTime.ToString();
                TempData["RouteInfoTotalTime"] = model.RouteInfo.TotalTime.ToString();
            }

            // Billing info
            if (model.FareBilling != null)
            {
                TempData["BillingSingleJourneyFare"] = model.FareBilling.SingleJourneyFare.ToString();
                TempData["BillingMrtPassFare"] = model.FareBilling.MrtPassFare.ToString();
                TempData["BillingMrtPassSavings"] = model.FareBilling.MrtPassSavings.ToString();
                TempData["BillingDistanceInStations"] = model.FareBilling.DistanceInStations.ToString();
            }

            TempData["ErrorMessage"] = model.ErrorMessage;
            TempData["SuccessMessage"] = model.SuccessMessage;
        }

        private RoutePlannerViewModel GetModelFromTempDataFallback()
        {
            var model = new RoutePlannerViewModel();

            // Current location
            if (TempData["CurrentLatitude"] != null)
            {
                if (double.TryParse(TempData["CurrentLatitude"]?.ToString(), out double lat) &&
                    double.TryParse(TempData["CurrentLongitude"]?.ToString(), out double lng))
                {
                    model.CurrentLatitude = lat;
                    model.CurrentLongitude = lng;
                }
                model.CurrentLocationText = TempData["CurrentLocationText"]?.ToString() ?? "Get Current Location";

                var stationName = TempData["NearestCurrentStationName"]?.ToString();
                var stationDistanceStr = TempData["NearestCurrentStationDistance"]?.ToString();
                if (!string.IsNullOrEmpty(stationName) &&
                    double.TryParse(stationDistanceStr, out double stationDistance))
                {
                    var station = _locationService.GetAllStations().FirstOrDefault(s => s.Name == stationName);
                    if (station != null)
                    {
                        model.NearestCurrentStation = new Station
                        {
                            Name = station.Name,
                            Latitude = station.Latitude,
                            Longitude = station.Longitude,
                            Distance = stationDistance
                        };
                    }
                }
            }

            // Destination
            model.DestinationQuery = TempData["DestinationQuery"]?.ToString() ?? string.Empty;
            if (TempData["SelectedDestinationName"] != null)
            {
                var destName = TempData["SelectedDestinationName"]?.ToString();
                var destDisplayName = TempData["SelectedDestinationDisplayName"]?.ToString();
                var destLatStr = TempData["SelectedDestinationLatitude"]?.ToString();
                var destLngStr = TempData["SelectedDestinationLongitude"]?.ToString();

                if (!string.IsNullOrEmpty(destName) && !string.IsNullOrEmpty(destDisplayName) &&
                    double.TryParse(destLatStr, out double destLat) &&
                    double.TryParse(destLngStr, out double destLng))
                {
                    model.SelectedDestination = new Place
                    {
                        Name = destName,
                        DisplayName = destDisplayName,
                        Latitude = destLat,
                        Longitude = destLng
                    };
                }

                var destStationName = TempData["NearestDestinationStationName"]?.ToString();
                var destStationDistanceStr = TempData["NearestDestinationStationDistance"]?.ToString();
                if (!string.IsNullOrEmpty(destStationName) &&
                    double.TryParse(destStationDistanceStr, out double destStationDistance))
                {
                    var station = _locationService.GetAllStations().FirstOrDefault(s => s.Name == destStationName);
                    if (station != null)
                    {
                        model.NearestDestinationStation = new Station
                        {
                            Name = station.Name,
                            Latitude = station.Latitude,
                            Longitude = station.Longitude,
                            Distance = destStationDistance
                        };
                    }
                }
            }

            // Origin
            model.OriginQuery = TempData["OriginQuery"]?.ToString() ?? string.Empty;
            if (TempData["SelectedOriginName"] != null)
            {
                var originName = TempData["SelectedOriginName"]?.ToString();
                var originDisplayName = TempData["SelectedOriginDisplayName"]?.ToString();
                var originLatStr = TempData["SelectedOriginLatitude"]?.ToString();
                var originLngStr = TempData["SelectedOriginLongitude"]?.ToString();

                if (!string.IsNullOrEmpty(originName) && !string.IsNullOrEmpty(originDisplayName) &&
                    double.TryParse(originLatStr, out double originLat) &&
                    double.TryParse(originLngStr, out double originLng))
                {
                    model.SelectedOrigin = new Place
                    {
                        Name = originName,
                        DisplayName = originDisplayName,
                        Latitude = originLat,
                        Longitude = originLng
                    };
                }

                var originStationName = TempData["NearestOriginStationName"]?.ToString();
                var originStationDistanceStr = TempData["NearestOriginStationDistance"]?.ToString();
                if (!string.IsNullOrEmpty(originStationName) &&
                    double.TryParse(originStationDistanceStr, out double originStationDistance))
                {
                    var station = _locationService.GetAllStations().FirstOrDefault(s => s.Name == originStationName);
                    if (station != null)
                    {
                        model.NearestOriginStation = new Station
                        {
                            Name = station.Name,
                            Latitude = station.Latitude,
                            Longitude = station.Longitude,
                            Distance = originStationDistance
                        };
                    }
                }
            }

            // Route info
            if (TempData["RouteInfoStations"] != null)
            {
                if (int.TryParse(TempData["RouteInfoStations"]?.ToString(), out int stations) &&
                    int.TryParse(TempData["RouteInfoMetroTime"]?.ToString(), out int metroTime) &&
                    int.TryParse(TempData["RouteInfoWalkingTime"]?.ToString(), out int walkingTime) &&
                    int.TryParse(TempData["RouteInfoTotalTime"]?.ToString(), out int totalTime))
                {
                    model.RouteInfo = new RouteInfo
                    {
                        Stations = stations,
                        MetroTime = metroTime,
                        WalkingTime = walkingTime,
                        TotalTime = totalTime,
                        OriginStation = model.NearestCurrentStation ?? model.NearestOriginStation,
                        DestinationStation = model.NearestDestinationStation
                    };

                    if (model.RouteInfo.OriginStation != null && model.RouteInfo.DestinationStation != null)
                    {
                        model.RouteInfo.RouteStations = _locationService.GetRouteStations(
                            model.RouteInfo.OriginStation, model.RouteInfo.DestinationStation);
                    }
                    else
                    {
                        model.RouteInfo.RouteStations = new List<Station>();
                    }
                }
            }

            // Billing info
            if (TempData["BillingSingleJourneyFare"] != null)
            {
                if (decimal.TryParse(TempData["BillingSingleJourneyFare"]?.ToString(), out decimal singleFare) &&
                    decimal.TryParse(TempData["BillingMrtPassFare"]?.ToString(), out decimal mrtPassFare) &&
                    decimal.TryParse(TempData["BillingMrtPassSavings"]?.ToString(), out decimal savings) &&
                    int.TryParse(TempData["BillingDistanceInStations"]?.ToString(), out int distance))
                {
                    model.FareBilling = new FareBilling
                    {
                        SingleJourneyFare = singleFare,
                        MrtPassFare = mrtPassFare,
                        MrtPassSavings = savings,
                        DistanceInStations = distance,
                        OriginStationName = model.RouteInfo?.OriginStation?.Name ?? "",
                        DestinationStationName = model.RouteInfo?.DestinationStation?.Name ?? "",
                        MrtPassDiscount = 10,
                        IsEligibleForMrtPass = true,
                        PaymentMethods = _billingService.GetPaymentMethods(singleFare, mrtPassFare)
                    };
                }
            }

            model.ErrorMessage = TempData["ErrorMessage"]?.ToString();
            model.SuccessMessage = TempData["SuccessMessage"]?.ToString();

            return model;
        }
    }
}
