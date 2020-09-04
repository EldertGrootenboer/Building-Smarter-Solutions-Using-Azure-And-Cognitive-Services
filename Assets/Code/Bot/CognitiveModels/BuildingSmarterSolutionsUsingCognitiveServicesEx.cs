// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Linq;

namespace Microsoft.BotBuilderSamples
{
    // Extends the partial BuildingSmarterSolutionsUsingCognitiveServices class with methods and properties that simplify accessing entities in the luis results
    public partial class BuildingSmarterSolutionsUsingCognitiveServices
    {
        public string Ship => Entities?.ship?.FirstOrDefault();
        public string VisitorName => Entities?.personName?.FirstOrDefault();
        public string VisitDateTime => Entities?.datetime?.FirstOrDefault()?.Expressions.FirstOrDefault();
        public string Reason => Entities?.reason?.FirstOrDefault();
        public string Email => Entities?.email?.FirstOrDefault();
    }
}
