namespace NMapper.TestData
{
    public sealed class Owner
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Car? Car { get; set; }
    }
}