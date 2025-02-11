using UnityEngine;

public class Movement : MonoBehaviour{
    // declaring a private instance variable
    public enum Type : int{
        None,
        Diagonal,
        Straight,
        Lion,
        Free,
        Jaguar,
        Hyena,

        SIZE
    }
}