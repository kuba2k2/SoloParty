namespace SoloParty.Data;

public enum Modifier
{
	// Game modifiers
	None = 0,
	BatteryEnergy = 1 << 0,
	NoFail = 1 << 1,
	InstaFail = 1 << 2,
	NoObstacles = 1 << 3,
	NoBombs = 1 << 4,
	FastNotes = 1 << 5,
	StrictAngles = 1 << 6,
	DisappearingArrows = 1 << 7,
	FasterSong = 1 << 8,
	SlowerSong = 1 << 9,
	NoArrows = 1 << 10,
	GhostNotes = 1 << 11,
	SuperFastSong = 1 << 12,
	ProMode = 1 << 13,
	ZenMode = 1 << 14,
	SmallCubes = 1 << 15,
	// Extra modifiers
	Multiplayer = 1 << 20,
}
