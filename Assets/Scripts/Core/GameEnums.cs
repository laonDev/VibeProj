namespace AnimalKitchen
{
    public enum GameState
    {
        Loading,
        MainMenu,
        Playing,
        Paused
    }

    public enum CustomerState
    {
        Entering,
        WaitingForSeat,
        WalkingToSeat,
        Ordering,
        WaitingForFood,
        Eating,
        Paying,
        Leaving
    }

    public enum StaffType
    {
        Chef,
        Waiter,
        Cashier
    }

    public enum StaffState
    {
        Idle,
        Working,
        Walking
    }

    public enum Rarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    public enum AnimalType
    {
        Cat,
        Dog,
        Rabbit,
        Pig,
        Raccoon,
        Hamster,
        Bear,
        Fox
    }

    public enum FoodCategory
    {
        Appetizer,
        MainDish,
        Dessert,
        Beverage
    }
}
