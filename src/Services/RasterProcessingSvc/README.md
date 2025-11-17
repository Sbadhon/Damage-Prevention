RasterProcessingService/
├── src/
│   ├── RasterProcessingService.Api/          # Web API layer
│   ├── RasterProcessingService.Application/ # Application services, DTOs
│   ├── RasterProcessingService.Domain/      # Core domain models & logic
│   └── RasterProcessingService.Infrastructure/ # AWS S3, GDAL, Logging

Layer Responsibilities

Domain Layer

    Core concepts: Raster, RasterBoundary, GeoJsonFeature

    Domain logic: calculate raster bounds, validate raster metadata

    Immutable, pure domain classes

Application Layer

    Use Cases / Services: UploadRasterService, GenerateGeoJsonService

    Handles orchestration between Domain and Infrastructure

    DTOs for communication with API

Infrastructure Layer

    External dependencies: AWS S3, GDAL, database if needed

    Implements interfaces defined in Domain/Application

    API Layer

    REST endpoints

    Converts HTTP requests to Application DTOs

    How It Works Now

    User uploads a real GeoTIFF raster.

    Backend reads the raster using GDAL, extracting:

    Raster size

    Georeference transform (origin + pixel size)

    Generates an exact bounding polygon of the raster in GeoJSON format.

    Uploads both the raster and GeoJSON to S3.

    This GeoJSON can be directly used in a frontend map (Google Maps, Leaflet, or ESRI JS API).