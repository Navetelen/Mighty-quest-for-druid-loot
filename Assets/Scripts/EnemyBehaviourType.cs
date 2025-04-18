using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyBehaviorType
{
    Walker,     // akadályokat kerül
    Flyer,      // alacsony akadályon átrepül
    Berserker,  // harcolni akar
    Support,    // lassú, gyógyít és támad
    Jumper      // átlépi az akadályokat
}