
namespace Plugin.Core.Models.Map
{
    public class MapRule
    {
        public string Name { get; set; }

        public int Id { get; set; }

        public int Rule { get; set; }

        public int StageOptions { get; set; }

        public int Conditions { get; set; }

        public MapRule() => this.Name = "";
    }
}
