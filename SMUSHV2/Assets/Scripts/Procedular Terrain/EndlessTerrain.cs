using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour {
    public const float maxViewDst = 450;
    public Transform viewer; // postition of viewer (what/whom the endless terrain will chase/track to get center position of terrain)

    public static Vector2 viewerPosition;
    private int chunkSize;
    private int chunksVissibleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrianChunksVisibleLastUpdate = new List<TerrainChunk>(); 

    void Start(){
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVissibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

    }
    void Update(){
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks(); 
    }
    void UpdateVisibleChunks(){
        for (int i = 0; i < terrainChunkDictionary.Count; i++) {
            terrianChunksVisibleLastUpdate[i].setVisible(false);
        }
        terrianChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVissibleInViewDst; yOffset <= chunksVissibleInViewDst; yOffset++) {
            for (int xOffset = -chunksVissibleInViewDst; xOffset <= chunkSize; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY +yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)) {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();

                    if (terrainChunkDictionary[viewedChunkCoord].IsVisible()) {
                        terrianChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                } else {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform)); 
                }
            }
        }
    }
    public class TerrainChunk {
        private GameObject meshObject;
        private Vector2 position;

        private Bounds bounds;

        public TerrainChunk(Vector2 coord, int size,Transform parent){
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 psostionV3 = new Vector3(position.x, 0, position.y);

            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = psostionV3;
            meshObject.transform.localScale = Vector3.one * size/ 10f;
            meshObject.transform.parent=parent;

            setVisible(false);
        }
        public void UpdateTerrainChunk (){
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDst;
            setVisible(visible);
        }

        public void setVisible(bool visible){
            meshObject.SetActive(visible);
        }
        public bool IsVisible(){
            return meshObject.activeSelf;
        }
    }
}
