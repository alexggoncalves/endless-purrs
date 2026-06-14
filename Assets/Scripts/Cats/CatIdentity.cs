using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BehaviourType
{
    Friendly,
    Scaredy
}

public class CatIdentity : MonoBehaviour
{
    // Start is called before the first frame update
    private static readonly string[] maleNames = {
        "Oliver", "Leo", "Milo", "Charlie", "Max", "Simba", "Jack", "Loki", "Oscar", "Toby",
        "Jasper", "George", "Sam", "Henry", "Buddy", "Gizmo", "Finn", "Apollo", "Merlin", "Oreo",
        "Felix", "Rocky", "Thor", "Zeus", "Archie", "Chester", "Smokey", "Buster", "Sammy", "Gus",
        "Frankie", "Hank", "Prince", "Bruno", "Percy", "Rusty", "Sarge", "Wally", "Joey", "Eddie",
        "Murphy", "Rex", "Scooter", "Marley", "Midnight", "Storm", "Whiskers", "Shadow", "Mittens", "Rascal",
        "Chase", "Cosmo", "Ziggy", "Diesel", "Boomer", "Blue", "Bandit", "Salem", "Hunter", "Frodo",
        "Spike", "Pablo", "Tigger", "Peanut", "Nico", "Romeo", "Bentley", "Zorro", "Winston", "Tiger",
        "Bear", "Mac", "Benny", "Ace", "Axel", "Cody", "Louie", "Otis", "Maverick", "Zane",
        "Ash", "Ranger", "Ryder", "Baxter", "Rambo", "Phoenix", "Socks", "Boots", "Blaze", "Rocco",
        "Monty", "Hobbes", "Simba", "Thor", "Zeus", "Tank", "Duke", "Lucky", "Benji", "Jax"
    };

    private static readonly string[] femaleNames = {
        "Luna", "Bella", "Lucy", "Kitty", "Chloe", "Lily", "Sophie", "Nala", "Daisy", "Cleo",
        "Molly", "Zoe", "Sasha", "Gracie", "Stella", "Lola", "Mia", "Abby", "Willow", "Penny",
        "Ginger", "Ruby", "Hazel", "Ellie", "Olivia", "Rosie", "Emma", "Jasmine", "Pearl", "Maddie",
        "Angel", "Coco", "Princess", "Snowball", "Peaches", "Pumpkin", "Callie", "Sandy", "Piper", "Honey",
        "Roxy", "Midnight", "Shadow", "Pepper", "Smokey", "Tinkerbell", "Millie", "Cupcake", "Sugar", "Maple",
        "Sienna", "Skye", "Raven", "Indie", "Violet", "Leia", "Bluebell", "Olive", "Misty", "Cinnamon",
        "Sabrina", "Zelda", "Lacey", "Jewel", "Fiona", "Athena", "Harley", "Mocha", "Nina", "Penelope",
        "Buttons", "Bonnie", "Velvet", "Poppy", "Trixie", "Cali", "Freya", "Gigi", "Joy", "Queen",
        "Fluffy", "Pearl", "Taffy", "Beauty", "Ariel", "Carmella", "Delilah", "Esme", "Izzy", "Kit",
        "Lulu", "Marley", "Matilda", "Minnie", "Opal", "Pandora", "Petunia", "Pixie", "Precious", "Rosy",
        "Sable", "Summer", "Tabitha", "Tasha", "Trudy", "Winnie", "Yuki", "Zara", "Dixie", "Gloria"
    };

    private static readonly List<string> usedMaleNames = new();
    private static readonly List<string> usedFemaleNames = new();

    public string catName;
    public string gender;
    public BehaviourType behaviour;

    public void SetIdentity()
    {
        // Set Gender
        gender = GetRandomGender();

        // Set Name
        catName = GetRandomName(gender);

        // Set Behaviour
        behaviour = GetRandomBehaviour();
    }

    public void SetIdentity(string name, string gender, BehaviourType behaviour, GameObject identityDisplay)
    {
        this.catName = name;
        this.gender = gender;
        this.behaviour = behaviour;
    }

    BehaviourType GetRandomBehaviour()
    {
        float random = Random.Range(0.0f, 1.0f);
        if (random < 0.5f)
        {
            return BehaviourType.Scaredy;
        }
        else
        {
            return BehaviourType.Friendly;
        }
    }

    string GetRandomGender()
    {
        // Set Gender
        float random = Random.Range(0.0f, 1.0f);

        if (random < 0.33f)
        {
            return "Female";
        }
        else if (random < 0.66f)
        {
            return "Male";
        }
        else {
            return "Neutral";
        }
    }

    public BehaviourType GetBehaviourType()
    {
        return behaviour;
    }

    string GetRandomName(string gender)
    {
        List<string> pool;
        List<string> used;

        switch (gender.ToLower())
        {
            case "male":
                pool = maleNames.ToList();
                used = usedMaleNames;
                break;
            default:
                pool = femaleNames.ToList();
                used = usedFemaleNames;
                break;
        }

        List<string> available = pool.Except(used).ToList();
        if (available.Count == 0) return "Unnamed";

        string selected = available[Random.Range(0, available.Count)];
        used.Add(selected);

        return selected;
    }

}
