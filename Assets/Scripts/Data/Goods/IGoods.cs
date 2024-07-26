namespace Data
{
    public interface IGoods
    {
        public void Add(long amount);
        public void Subtract(long amount);
        public void Set(long amount);

        public long Get();
    }
}