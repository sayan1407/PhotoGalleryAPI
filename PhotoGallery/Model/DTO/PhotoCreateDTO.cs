﻿namespace PhotoGallery.Model.DTO
{
    public class PhotoCreateDTO
    {
        public IFormFile file { get; set; }
        public string UserId { get; set; }
    }
}
