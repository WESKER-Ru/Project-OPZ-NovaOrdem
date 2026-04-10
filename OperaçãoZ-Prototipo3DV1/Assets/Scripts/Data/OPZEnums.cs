// Assets/Scripts/Data/OPZEnums.cs
namespace OPZ.Data
{
    public enum Faction { AR, EG }

    public enum ResourceType { Supplies, Metal, Fuel }

    public enum UnitState
    {
        Idle, Move, Gather, ReturnToDepot, Build, Farm, Flee, ResumePreviousCommand,
        Attack, Chase, Hold, AutoDefend, ReturnToCommand, Dead
    }

    public enum BuildingState
    {
        Ghost, Foundation, UnderConstruction, Built, Disabled, Destroyed
    }

    public enum UnitRole { Worker, Infantry, Ranged, Scout }

    public enum InfectedType { Vagante, Corredor }

    public enum CommandType { Move, Attack, Gather, Build, Patrol, Stop, HoldPosition }
}
