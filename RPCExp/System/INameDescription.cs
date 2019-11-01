using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace System
{
    public interface INameDescription
    {
        [MaxLength(64)]
        [Required]
        string Name { get; set; }

        [MaxLength(512)]
        string Description { get; set; }
    }

    //public static class NameDescriptionExtention
    //{
    //    public static string AccessNameGet(this INameDescription obj)
    //    {
    //        return obj.Name
    //            .Replace("\t"," ")
    //            .Replace("\r\n", " ")
    //            .Replace("\r", " ")
    //            .Replace("\n", " ")
    //            .Replace("      ", " ")
    //            .Replace("     ", " ")
    //            .Replace("    ", " ")
    //            .Replace("   ", " ")
    //            .Replace("  ", " ")
    //            .Replace("  ", " ")
    //            .Replace(" ", "+") ;
    //    }
    //}
}
