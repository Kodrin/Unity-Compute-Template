using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class ParticleMaster : MonoBehaviour
{
    public enum ParticleAmount
    {
        OneK = 1024,
        TwoK = 2048,
        FourK = 4096,
        EightK = 8192,
        SixTeenK = 16384,
        ThirtyTwoK = 32768
    }
    [SerializeField] protected ParticleAmount NUM_PARTICLES = ParticleAmount.FourK;
    const int NUM_THREAD_X = 1024; 
    const int NUM_THREAD_Y = 1; 
    const int NUM_THREAD_Z = 1;


    #region Compute Parameters
    public Camera RenderCam;
    public Vector3 spawnArea = new Vector3(5,5,5);
    public Vector3 particleDirection = new Vector3(0,1,0);
    public float particleSize = 1;
    public Mesh BoidMesh;

    #endregion

    #region Private
    public ComputeShader particleComputeShader;
    public Shader particleRenderShader;
    ComputeBuffer particleComputeBuffer;
    ComputeBuffer _drawArgsBuffer;
    Material particleRenderMaterial;
    #endregion 

    // Start is called before the first frame update
    void Start()
    {
                // Initialize the indirect draw args buffer.
        _drawArgsBuffer = new ComputeBuffer(
            1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments
        );

        _drawArgsBuffer.SetData(new uint[5] {
            BoidMesh.GetIndexCount(0), (uint) NUM_PARTICLES, 0, 0, 0
        });
        GenerateParticleBuffer();

        //create the material from the shader 
        particleRenderMaterial = new Material(particleRenderShader);
        particleRenderMaterial.hideFlags = HideFlags.HideAndDontSave;
    }

    // Update is called once per frame
    void Update()
    {
        ComputeParticles(); //update the data in the buffers 
    }

    void OnRenderObject()
    {
        RenderParticles();
    }

    void OnDestroy()
    {
        //Clear the buffers
        if(particleComputeBuffer != null)
        {
            particleComputeBuffer.Release();
            particleComputeBuffer = null;   
        }

        if (_drawArgsBuffer != null) _drawArgsBuffer.Release();

    }
    ParticleData CreateParticleData()
    {
        ParticleData p = new ParticleData();
        p.position.x = Random.insideUnitSphere.x * spawnArea.x;
        p.position.y = Random.insideUnitSphere.y * spawnArea.y;
        p.position.z = Random.insideUnitSphere.z * spawnArea.z;
        p.velocity = Random.Range(-1.0f,1.0f) * Vector3.one;
        p.size = Random.value * particleSize;
        return p;
    }

    void GenerateParticleBuffer()
    {
        // buffer to store our particles in 
        particleComputeBuffer = new ComputeBuffer((int)NUM_PARTICLES, Marshal.SizeOf(typeof(ParticleData)));
        var pData = new ParticleData[(int)NUM_PARTICLES];

        for (int i = 0; i < pData.Length; i++)
        {
            pData[i] = CreateParticleData();
        }
        
        particleComputeBuffer.SetData(pData); // set data into buffer
        pData = null; //clear it
    }

    void ComputeParticles()
    {
            ComputeShader cs = particleComputeShader;

            // Part. num divided into thread groups
            int numThreadGroup = (int)NUM_PARTICLES / NUM_THREAD_X;

            // Find Main function
            int kernelId = cs.FindKernel("CSMain");

            var inverseViewMatrix = RenderCam.worldToCameraMatrix.inverse;
            Material m = particleRenderMaterial;
            // m.SetPass(0); 
            // m.SetMatrix("_InvViewMatrix", inverseViewMatrix);
            m.SetBuffer("boidBuffer", particleComputeBuffer);

            // set the particle buffer
            cs.SetBuffer(kernelId, "_ParticleBuffer", particleComputeBuffer);
            
		    // execute the result (sends all these operations to the compute)
            cs.Dispatch(kernelId, numThreadGroup, 1, 1);
            // Graphics.DrawProceduralNow(MeshTopology.Points, (int)NUM_PARTICLES);
                    Graphics.DrawMeshInstancedIndirect(
                BoidMesh, 0, particleRenderMaterial,
                new Bounds(Vector3.zero, Vector3.one * 1000),
                _drawArgsBuffer, 0
            );
    }

    void RenderParticles()
    {
        // m.SetColor("", );

    }
}
