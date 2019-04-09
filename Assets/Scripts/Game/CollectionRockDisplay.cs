using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CollectionRockDisplay : MonoBehaviour {
    public enum RockType {
        Igneous,
        Sedimentary,
        Metamorphic
    }

    public InventoryData inventory;
    public RockType rockType;
    public SpriteShapeController[] spriteShapes;

    void OnEnable() {
        RockData[] rocks;

        //go through seen rocks
        switch(rockType) {
            case RockType.Igneous:
                rocks = inventory.rocksIgneous;
                break;
            case RockType.Sedimentary:
                rocks = inventory.rocksSedimentary;
                break;
            case RockType.Metamorphic:
                rocks = inventory.rocksMetamorphic;
                break;
            default:
                rocks = null;
                break;
        }

        if(rocks != null) {
            int seenCount = 0;

            for(int i = 0; i < rocks.Length; i++) {
                var rock = rocks[i];
                if(rock.isSeen) {
                    var spriteShape = spriteShapes[seenCount];
                    if(spriteShape) {
                        spriteShape.gameObject.SetActive(true);
                        spriteShape.spriteShape = rock.spriteShape;
                    }

                    seenCount++;
                    if(seenCount == spriteShapes.Length)
                        break;
                }
            }

            for(int i = seenCount; i < spriteShapes.Length; i++) {
                if(spriteShapes[i])
                    spriteShapes[i].gameObject.SetActive(false);
            }
        }
    }
}
