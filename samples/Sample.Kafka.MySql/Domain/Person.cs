namespace Sample.Kafka.MySql.Domain
{
    public class Person
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return $"Name:{Name}, Id:{Id}";
        }
    }
}