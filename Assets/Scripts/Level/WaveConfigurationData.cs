using StarSalvager.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveConfigurationData
{
    //Time Duration
    public int WaveDuration;
    //Width of grid on this wave
    public int GridWidth;

    //Hadn't fully planned out this part yet. The StageEnemyData value likely either needs to be altered or a different struct needs to be used. This should represent the list of enemies that can be used in this wave, as well as the probability of each of them occurring.
    public List<StageEnemyData> StageEnemyData;
    //This represents the total enemy budget that can be spawned in this wave. Enemy budget hasn't been implemented yet, the plan was to have enemies default to a value of 1.0f that can be overridden as necessary.
    public int EnemyBudget;

    //Overall # of bits dropped per minute. Was going to port in the density math from the current wave systems
    public int BitsPerMinute;
    //The next five variables should collectively add up to 1.0, and represent the portion of the above value bits per minute that are this colour
    public float RedBitsPercentage;
    public float BlueBitsPercentage;
    public float GreenBitsPercentage;
    public float YellowBitsPercentage;
    public float GreyBitsPercentage;
}
