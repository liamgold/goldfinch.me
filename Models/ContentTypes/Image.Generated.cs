// This code was generated by a kontent-generators-net tool 
// (see https://github.com/Kentico/kontent-generators-net).
// 
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated. 
// For further modifications of the class, create a separate file with the partial class.

using System;
using System.Collections.Generic;
using Kentico.Kontent.Delivery.Abstractions;

namespace Goldfinch.Models.ContentTypes
{
    public partial class Image
    {
        public const string Codename = "image";
        public const string ImageassetCodename = "imageasset";

        public IEnumerable<IAsset> Imageasset { get; set; }
        public IContentItemSystemAttributes System { get; set; }
    }
}