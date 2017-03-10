namespace Common
{
    public enum OrderType
    {
        Drink,
        Sparkling,
        Beer
    }

    public enum ActivityType
    {
        ProcessOrders,
        IdleDemo,
        Macro,
        Idle
    }

    public enum RunMode
    {
        Simulation,
        Production
    }

    public enum State
    {
        Initialize,
        InitFailure,
        Idle,
        IdleDemo,
        GrabBottle,
        GetNewBottle,
        ReturnBottle,
        RemoveBottle,
        DisposeBeer,
        PourDrinks,
        PourBeer,
        PourSparkling
    }
}
