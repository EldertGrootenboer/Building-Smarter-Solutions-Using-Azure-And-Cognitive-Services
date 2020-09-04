using System.Collections.Generic;

namespace EPH.Functions
{
    public class ModelsResponse
    {
        public List<Model> modelList { get; set; }
        public string nextLink { get; set; }
    }
}