using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// interface: Any class that inherits from an interface must define its declared members.
public interface IRootState 
{
    // Grounded, Fall, Jump will inherit from IrootState and PlayerBaseState
    void HandleGravity();
    
}
