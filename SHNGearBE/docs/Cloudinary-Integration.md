# Cloudinary Integration Guide

## 1. Scope
This document summarizes what has been implemented for Cloudinary image upload in `SHNGearBE`, what features are available, and common success/error cases.

## 2. What Was Implemented
The backend now supports uploading images to Cloudinary and returns secure URLs so business modules can store only image URLs.

### Added/Updated Files
- `SHNGearBE/SHNGearBE.csproj`
  - Added `CloudinaryDotNet` package.
- `SHNGearBE/Configurations/CloudinarySettings.cs`
  - Cloudinary config model (`CloudName`, `ApiKey`, `ApiSecret`).
- `SHNGearBE/Extensions/ServiceCollectionExtensions.cs`
  - Added Cloudinary settings binding from config/environment.
  - Registered image storage service in DI.
- `SHNGearBE/Program.cs`
  - Added Cloudinary settings registration.
- `SHNGearBE/Infrastructure/Media/CloudinaryImageStorageService.cs`
  - Core Cloudinary upload implementation and file validation.
- `SHNGearBE/Services/Interfaces/Media/IImageStorageService.cs`
  - Media upload service contract.
- `SHNGearBE/Controllers/MediaController.cs`
  - New upload API endpoint.
- `SHNGearBE/Models/DTOs/Media/ImageUploadResponse.cs`
  - Upload response DTO.
- `SHNGearBE/appsettings.json`
- `SHNGearBE/appsettings.Development.json`
  - Added `Cloudinary` config section placeholder.
- `docker-compose.yml`
  - Injected Cloudinary env vars into backend container.
- `SHNGearBE/.env`
  - Cloudinary environment values.

## 3. Features Available
### 3.1 Upload Multiple Images
- Endpoint accepts multiple files in one request.
- Returns a list of uploaded image metadata.

### 3.2 Secure URL Output
- Uses Cloudinary secure URL.
- Returned URL is ready to store in DB (`ProductImages.Url`) and serve directly in frontend.

### 3.3 Optional Folder Routing
- Supports optional `folder` form field.
- If omitted, default folder is `shn-gear`.

### 3.4 Input Validation
- Allowed mime types:
  - `image/jpeg`
  - `image/png`
  - `image/webp`
  - `image/gif`
  - `image/avif`
- Max file size: `10MB` per file.
- Empty file list or empty file is rejected.

### 3.5 Permission Protection
- Upload endpoint is protected by:
  - `RequirePermission(Permissions.EditProduct)`
- This aligns with admin/product-management workflow.

## 4. API Spec
### Endpoint
- `POST /api/Media/images`

### Auth
- Required permission: `products.edit`

### Content Type
- `multipart/form-data`

### Form Fields
- `files` (required): image file list.
- `folder` (optional): Cloudinary folder.

### Success Response (200)
```json
[
  {
    "url": "https://res.cloudinary.com/<cloud>/image/upload/.../abc.jpg",
    "publicId": "shn-gear/abc",
    "bytes": 345678,
    "format": "jpg"
  }
]
```

## 5. Integration Case With Product
### Recommended Flow
1. Frontend uploads images via `POST /api/Media/images`.
2. Frontend receives uploaded `url` values.
3. Frontend calls product create/update API with `ImageUrls`.
4. Backend stores these URLs in `ProductImages.Url`.

This keeps Product service independent from file-storage provider.

## 6. Cases and Behaviors
### Case A: Valid Upload (Single/Multiple)
- Input: valid image files with supported mime type and size <= 10MB.
- Result: success, returns uploaded URL list.

### Case B: Unsupported Mime Type
- Input: file with unsupported content type.
- Result: `ProjectException(ResponseType.InvalidData)`.
- Message indicates accepted image formats.

### Case C: File Too Large
- Input: image larger than 10MB.
- Result: `ProjectException(ResponseType.InvalidData)`.

### Case D: Empty File / Empty File List
- Input: no files or zero-byte file.
- Result: `ProjectException(ResponseType.ImageCannotBeEmpty)`.

### Case E: Missing Cloudinary Config
- Input: Cloudinary config not set (`CloudName/ApiKey/ApiSecret`).
- Result: runtime configuration exception when resolving service.
- Fix: provide env/config values.

### Case F: Unauthorized / Missing Permission
- Input: token without `products.edit` permission.
- Result: authorization failure (`401` or `403` depending on auth state).

### Case G: Cloudinary Service Error
- Input: Cloudinary rejects upload or network issue.
- Result: `ProjectException(ResponseType.ServiceUnavailable)`.

## 7. Configuration
### Required Variables
- `CLOUDINARY_CLOUD_NAME`
- `CLOUDINARY_API_KEY`
- `CLOUDINARY_API_SECRET`

### Resolution Priority
Values are bound from `Cloudinary` section, then missing fields fallback to environment variables.

## 8. Example Test Command
```bash
curl -X POST "http://localhost:5000/api/Media/images" \
  -H "Authorization: Bearer <token>" \
  -F "files=@./sample-1.jpg" \
  -F "files=@./sample-2.png" \
  -F "folder=products"
```

## 9. Notes
- Current implementation focuses on upload only.
- Delete image by `publicId` is not implemented yet.
- If needed, add a delete endpoint and cleanup flow when replacing/removing product images.
