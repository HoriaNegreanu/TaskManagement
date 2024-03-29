﻿using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagement.Models
{
    public class FileOnFileSystemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FileType { get; set; }
        public string Extension { get; set; }
        public string Description { get; set; }
        public string UploadedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string FilePath { get; set; }

        [NotMapped]
        public string NameWithExtension
        {
            get { return string.Format("{0}{1}", Name, Extension); }
        }
    }
}
