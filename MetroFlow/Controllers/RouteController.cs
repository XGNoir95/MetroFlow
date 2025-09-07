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

        public RouteController(ILogger<RouteController> logger, ILocationService locationService)
        {
            _logger = logger;
            _locationService = locationService;
        }

        [HttpGet]
        public IActionResult RoutePlanner()
        {
            var model = GetModelFromSession() ?? new RoutePlannerViewModel
            {
                AllStations = _locationService.GetAllStations()
            };

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

            model.ErrorMessage = TempData["ErrorMessage"]?.ToString();
            model.SuccessMessage = TempData["SuccessMessage"]?.ToString();

            return model;
        }
    }
}
