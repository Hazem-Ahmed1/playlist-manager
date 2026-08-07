/** Mirrors PlaylistManagement.Api.DTOs.Common.ApiResponse&lt;T&gt;. Every successful API response is shaped like this. */
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

/** Mirrors PlaylistManagement.Api.DTOs.Common.ApiErrorResponse. Every failed API response (validation or otherwise) is shaped like this. */
export interface ApiErrorResponse {
  success: boolean;
  message: string;
  errors: ApiValidationError[];
}

/** Mirrors PlaylistManagement.Api.DTOs.Common.ApiValidationError. */
export interface ApiValidationError {
  field: string;
  message: string;
}
