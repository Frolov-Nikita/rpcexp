using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace System
{
    public interface INameDescription
    {
        [MaxLength(64)]
        string Name { get; set; }

        [MaxLength(512)]
        string Description { get; set; }
    }
}
