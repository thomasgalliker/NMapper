namespace NMapper.TestData
{
    public sealed class Car
    {
        public int Id { get; set; }

        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Plain nested reference without a back-reference, so the graph has non-cyclic depth too.
        /// </summary>
        public Brand? Brand { get; set; }

        /// <summary>
        /// Back-reference closing the <see cref="TestData.Garage"/> to <see cref="Car"/> cycle,
        /// which runs through a collection.
        /// </summary>
        public Garage? Garage { get; set; }

        /// <summary>
        /// Back-reference closing the <see cref="TestData.Owner"/> to <see cref="Car"/> cycle,
        /// which runs purely through single references.
        /// </summary>
        public Owner? Owner { get; set; }
    }
}