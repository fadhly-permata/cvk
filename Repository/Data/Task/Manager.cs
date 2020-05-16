namespace Repository.Data.Task
{
    public class Manager : EntityManager
    {
        public Manager(EntityManagerInitiateModel data) : base(data)
        {
            Reader = new Reader(this);
            Creator = new Creator(this);
            Updater = new Updater(this);
            Deleter = new Deleter(this);
        }

        ~Manager()
        {
            Dispose(false);
        }

        public Creator Creator { get; internal set; }
        public Deleter Deleter { get; internal set; }
        public Reader Reader { get; internal set; }
        public Updater Updater { get; internal set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}