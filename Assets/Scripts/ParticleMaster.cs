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

    #endregion

    #region Private
    public ComputeShader particleComputeShader;
    public Shader particleRenderShader;
    ComputeBuffer particleComputeBuffer;
    Material particleRenderMaterial;
    #endregion 

    // Start is called before the first frame update
    void Start()
    {
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
    }
    ParticleData CreateParticleData()
    {
        ParticleData p = new ParticleData();
        p.position = Random.value * spawnArea;
        p.velocity = Random.value * Vector3.one;
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


            // set the particle buffer
            cs.SetBuffer(kernelId, "_ParticleBuffer", particleComputeBuffer);
            
		    // execute the result (sends all these operations to the compute)
            cs.Dispatch(kernelId, numThreadGroup, 1, 1);
    }

    void RenderParticles()
    {
        var inverseViewMatrix = RenderCam.worldToCameraMatrix.inverse;
        Material m = particleRenderMaterial;
        m.SetPass(0); 
        m.SetMatrix("_InvViewMatrix", inverseViewMatrix);
        m.SetBuffer("_ParticleBuffer", particleComputeBuffer);
        // m.SetColor("", );

        Graphics.DrawProceduralNow(MeshTopology.Points, (int)NUM_PARTICLES);
    }
}
