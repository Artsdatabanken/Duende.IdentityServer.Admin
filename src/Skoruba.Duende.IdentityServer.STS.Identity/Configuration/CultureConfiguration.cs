﻿// Copyright (c) Jan Škoruba. All Rights Reserved.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;

namespace Skoruba.Duende.IdentityServer.STS.Identity.Configuration
{
    public class CultureConfiguration
    {
        public static readonly string[] AvailableCultures = { "en", "nb", "fa", "fr", "ru", "sv", "zh", "es", "da", "de", "nl", "fi", "pt" };
        public static readonly string DefaultRequestCulture = "nb";

        public List<string> Cultures { get; set; }
        public string DefaultCulture { get; set; } = DefaultRequestCulture;
    }
}
