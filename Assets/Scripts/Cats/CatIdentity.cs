
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public enum BehaviourType
{
    Owned,
    Friendly,
    Scaredy
}

public class CatIdentity : MonoBehaviour
{
    // Start is called before the first frame update
    private static string[] maleNames = {
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

    private static string[] femaleNames = {
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

    private static List<string> usedMaleNames = new List<string>();
    private static List<string> usedFemaleNames = new List<string>();

    public string catName;
    public string gender;
    public BehaviourType behaviour;

    private bool displayEnabled = false;
    private GameObject identityDisplay = null;

    public LayerMask ignoreLayerMask;
    public void SetIdentity(GameObject identityDisplay)
    {
        // Set Gender
        gender = GetRandomGender();

        // Set Name
        catName = GetRandomName(gender);

        // Set Behaviour
        behaviour = GetRandomBehaviour();
        this.identityDisplay = identityDisplay;

    }

    private void FixedUpdate()
    {
        identityDisplay.transform.rotation = Quaternion.identity;
    }

    public void SetIdentity(string name, string gender, BehaviourType behaviour, GameObject identityDisplay)
    {
        this.catName = name;
        this.gender = gender;
        this.behaviour = behaviour;
        this.identityDisplay = identityDisplay;
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

    private void LateUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform the raycast, using the layer mask to ignore specific layers
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~ignoreLayerMask))
        {
            // The raycast hit an object that is not in the ignored layer
            Debug.Log("Hit: " + hit.collider.name);
        }
        else
        {
            // No hit, or hit an object in the ignored layer
            Debug.Log("No hit, or hit an ignored object.");
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
        List<string> selectedList;
        List<string> usedList;
        string selectedName = "No name";

        switch (gender.ToLower())
        {
            case "male":
                usedList = usedMaleNames;
                selectedList = maleNames.ToList();
                break;
            case "female":
                usedList = usedFemaleNames;
                selectedList = femaleNames.ToList();
                break;
            default:
                usedList = usedFemaleNames.Concat(usedMaleNames).ToList();
                selectedList = femaleNames.Concat(maleNames).ToList();
                break;
        }

        if(selectedList.Count > 0)
        {
            bool valid = false;
            while (!valid)
            {
                int randomIndex = Random.Range(0, selectedList.Count);

                if (!usedList.Contains(selectedList[randomIndex])){
                    selectedName = selectedList[randomIndex];
                    usedList.Add(selectedName);
                    break;
                }
            }
        }

        return selectedName;
    }



    private void Update()
    {
        
    }
/*
    private void OnMouseEnter()
    {
        displayEnabled = true;
        identityDisplay.SetActive(true);
    }

    private void OnMouseExit()
    {
        displayEnabled = false;
        identityDisplay.SetActive(false);
    }*/
}
