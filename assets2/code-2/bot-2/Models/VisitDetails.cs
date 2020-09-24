// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.BotBuilderSamples.Models
{
    public class VisitDetails
    {
        public string Ship { get; set; }

        public Visitor Visitor { get; set; }

        public Visit Visit { get; set; }
    }

    public class Visit
    {
        public string DateTime { get; set; }

        public string Reason { get; set; }

        public bool RegistrationCompleted { get; set; }
    }

    public class Visitor
    {
        public string Name { get; set; }

        public VisitorPicture Picture { get; set; }

        public string Email { get; set; }

        public bool IsNew { get; set; }
    }

    public class VisitorPicture
    {
        public string Uri { get; set; }

        public string ContentType { get; set; }
    }
}