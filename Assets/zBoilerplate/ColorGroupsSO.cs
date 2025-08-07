using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ColorGroupsSO", menuName = "Scriptable Objects/ColorGroupsSO")]
public class ColorGroupsSO : ScriptableObject
{
    public List<Material> matList = new List<Material>(5);

    public List<ColorGroup> colorGroups = new List<ColorGroup>();


    public ColorGroup GetRandomColorGroup(ColorGroup currentGroup)
    {
        List<ColorGroup> listCopy = new List<ColorGroup>(colorGroups);
        listCopy.Remove(currentGroup);
        return listCopy[Random.Range(0, listCopy.Count)];
    }
}





[System.Serializable]
public class ColorGroup
{
    public string name;
    public List<Color> colors;

    public ColorGroup(List<Color> colors)
    {
        this.colors = colors;
    }

    public List<Color> GetColors()
    {
        List<Color> listCopy = new List<Color>(colors);
        return listCopy;
    }


    // {
    //     //    Initialise(noOfMaterials);
    // }
    // public void Initialise(int noOfMaterials)
    // {
    //     if (colors.Count == 0)
    //     {
    //         colors = new List<Color>(noOfMaterials);
    //     }

    //     if (colors.Count != 0 && colors.Count < noOfMaterials)
    //     {
    //         colors.Add(Color.black);
    //     }

    //     if (colors.Count != 0 && colors.Count > noOfMaterials)
    //     {
    //         int toRemove = colors.Count - noOfMaterials;
    //         for (int i = 0; i < toRemove; i++)
    //         {
    //             colors.RemoveAt(colors.Count - 1);
    //         }
    //     }
    // }
}


