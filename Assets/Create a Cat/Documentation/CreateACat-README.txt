   _.---.._             _.---...__    ____                _        
.-'   /\   \          .'  /\     /  / ___|_ __ ___  __ _| |_ ___  
`.   (  )   \        /   (  )   /  | |   | '__/ _ \/ _` | __/ _ \ 
  `.  \/   .'\      /`.   \/  .'   | |___| | |  __/ (_| | ||  __/ 
    ``---''   )    (   ``---''      \____|_|  \___|\__,_|\__\___| 
            .';.--.;`.                  __ _           
          .' /_...._\ `.               / _` |          
        .'   `.a  a.'   `.            | (_| |          
       (        \/        )            \__,_|          
        `.___..-'`-..___.'              ____      _   
           \          /                / ___|__ _| |_ 
            `-.____.-'                | |   / _` | __|
                                      | |__| (_| | |_ 
                                       \____\__,_|\__|

Create a Cat is a Unity editor tool for creating procedural cats of varying shapes, sizes and with procedural patterns.
It is able to create unique cats using perlin noise patterning and with randomized shape keys. It is able to save created
cats as prefabs, and settings objects can be saved to generate similar cats.

All cats are rigged using a cat armature and provide a basic amount of animations by default through a cat animator.
Additionally, a few pre-created cat prefabs are included in the Prefabs/Cat Examples folder.

---- HOW TO USE ----
The easiest way to begin generating cats is by simply instantiating the cat generator prefab into your scene. It is already
set up with the required hierarchy and fields, and ready to start generating!

Otherwise, add the cat generator component onto a desired GameObject. Below this, you need to add the cat mesh and armature,
both of which are included in the base cat model. Add these as children of the generator GameObject, and then assign the
following fields:
    * Create a Cat Material
        - This should always be the CreateACat Material (unless you are making your own custom cat material)
    * Cat SMR
        - The skinned mesh renderer you want to become the cat mesh (this should be a child of the cat generator)
For a complete reference of how the cat generator is set up, look at the cat generator prefab and its child components.

From here, the cat settings ScriptableObject should be assigned or created. Once this field is populated, the generation
buttons below should become available. The generator provides the following buttons:

    [Save Settings]
        - Creates a cat settings ScriptableObject asset in the CreateACat Exports folder
    [Create Cat]
        - Creates a new cat instance, randomizes the blendshapes and material parameters
    [Randomize Cat]
        - Randomizes the cat blendshapes and material parameters with a randomized seed
    [Save Cat Prefab]
        - Creates a prefab of the current cat in the CreateACat Exports folder

---- SETTINGS ----
The following is a list of all the settings provided by Create a Cat and a description of their effects:

    [Cat Settings]
        * Coat Colours
            - A collection of colours which the cat generator will choose from at random to define the cat's coat colour
        * Eye Colours
            - A collection of colours which the cat generator will choose from at random to define the cat's eye colour
        * Skin Colours
            - A collection of colours which the cat generator will choose from at random to define the cat's skin colour
        * Cat Accessories
            - A collection of accessory prefabs which the cat generator might choose from to make an accessory
        * Accessory Chance
            - The integer chance that a cat will spawn with an accessory

    [Cat Blendshapes]
        * Chonk
            - Controls how heavyset the cat is
        * Ear Height
            - Controls how tall the cat's ears are
        * Ear Width
            - Controls how wide the cat's ears are
        * Eye Height
            - Controls how tall the cat's eyes are
        * Eye Width
            - Controls how wide the cat's eyes are
        * Head Size
            - Controls the overall size of the cat's head
        * Paw Size
            - Controls the overall size of all of the cat's paws
        * Tail Length
            - Controls the overall length of the cat's tail
        * Tail Thickness
            - Controls the thickness / width of the cat's tail
        * Whisker Curvature
            - Controls the direction the cat's whiskers will curve
        * Whisker Length
            - Controls the overall length of the cat's whiskers
        * Whicker Thickness
            - Controls the overall thickness of the cat's whiskers

    [Cat Material Parameters]
        * Coat Colour
            - The base colour used to define the cat's coat colour and to some extent the pattern colour
        * Eye Colour
            - The colour being used to colourize the cat's eyes
        * Pattern Offset
            - An offset vector to apply to the noise sample position creating the pattern
        * Pattern Threshold [0, 1]
            - A threshold step from which the pattern should be visible
        * Spots Pattern
            - A simple toggle whether a spots pattern should be generated, if false, stripes are generated
        * Skin Colour
            - The colour used to colourize skin areas such as ears, nose, and paws
        * Undercoat Colour
            - Not randomized, only colourizes the undercoat areas if user enters a colour
        * Undercoat Leg Coverage [0, 1]
            - How high up the cat's legs undercoat colour should cover
        * Undercoat Tail Coverage [0, 1]
            - How low down the cat's tail undercoat colour should cover
        * Undercoat Threshold [0, 1]
            - How much of the cat's mid-section undercoat coverage should be visible

Hopefully Create a Cat allows you generate lots of unique and interesting cats for whatever project you use it in!
