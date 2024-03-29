﻿using System;
using System.Collections.Generic;
using RollerCaster.Collections;

namespace RollerCaster.Data
{
    public interface IProduct
    {
        string Name { get; set; }

        int Ordinal { get; set; }

        double Price { get; set; }

        DateTime CreatedOn { get; set; }

        ICollection<string> Categories { get; }

        IDictionary<string, string> Properties { get; }

        ReadOnlySpecializedCollection Keywords { get; }
    }
}
