//----------------------------------------------------------------------------
//  Copyright (C) 2004-2011 by EMGU. All rights reserved.       
//----------------------------------------------------------------------------

﻿using System;
using System.Collections.Generic;
using System.Text;
using Emgu.Util;
using System.Runtime.InteropServices;
using Emgu.CV.Features2D;

namespace Emgu.CV.GPU
{
   /// <summary>
   /// A Brute force matcher using GPU
   /// </summary>
   /// <typeparam name="T">The type of data to be matched. Can be either float or Byte</typeparam>
   public class GpuBruteForceMatcher <T> : UnmanagedObject
      where T: struct
   {
      private DistanceType _distanceType;

      /// <summary>
      /// Create a GPUBruteForce Matcher using the specific distance type
      /// </summary>
      /// <param name="distType">The distance type</param>
      public GpuBruteForceMatcher(DistanceType distType)
      {
         if (distType == DistanceType.Hamming) 
            distType = DistanceType.HammingLUT;
         
         if (typeof(T) == typeof(byte))
         {
            if (distType != DistanceType.HammingLUT)
               throw new ArgumentException("Hamming distance type requires model descriptor to be Matrix<Byte>");
         }
         else if (typeof(T) == typeof(float))
         {
            if (!(distType == DistanceType.L2F32 || distType == DistanceType.L1F32))
               throw new ArgumentException("L1 / L2 distance type requires model descriptor to be Matrix<float>");
         }
         else
         {
            throw new NotImplementedException(String.Format("Data type of {0} is not supported", typeof(T).ToString()));
         }

         _distanceType = distType;
         _ptr = GpuBruteForceMatcherInvoke.gpuBruteForceMatcherCreate(distType);
      }

      /*
      /// <summary>
      /// Add the model descriptors
      /// </summary>
      /// <param name="modelDescriptors">The model discriptors</param>
      public void Add(Matrix<Byte> modelDescriptors)
      {
         if (!(_distanceType == DistanceType.HammingDist))
            throw new ArgumentException("Hamming distance type requires model descriptor to be Matrix<Byte>");
         gpuBruteForceMatcherAdd(_ptr, modelDescriptors);
      }

      /// <summary>
      /// Add the model descriptors
      /// </summary>
      /// <param name="modelDescriptors">The model discriptors</param>
      public void Add(Matrix<float> modelDescriptors)
      {
         if (!(_distanceType == DistanceType.L2 || _distanceType == DistanceType.L1))
            throw new ArgumentException("L1 / L2 distance type requires model descriptor to be Matrix<float>");
         gpuBruteForceMatcherAdd(_ptr, modelDescriptors);
      }*/

      /// <summary>
      /// Find the k nearest neighbour using the brute force matcher. 
      /// </summary>
      /// <param name="queryDescriptors">The query descriptors</param>
      /// <param name="modelDescriptors">The model descriptors</param>
      /// <param name="modelIdx">The model index. A n x <paramref name="k"/> matrix where n = <paramref name="queryDescriptors"/>.Cols</param>
      /// <param name="distance">The matrix where the distance valus is stored. A n x <paramref name="k"/> matrix where n = <paramref name="queryDescriptors"/>.Size.Height</param>
      /// <param name="k">The number of nearest neighbours to be searched</param>
      /// <param name="mask">The mask</param>
      public void KnnMatch(GpuMat<T> queryDescriptors, GpuMat<T> modelDescriptors, GpuMat<int> modelIdx, GpuMat<float> distance, int k, GpuMat<Byte> mask)
      {
         GpuBruteForceMatcherInvoke.gpuBruteForceMatcherKnnMatch(_ptr, queryDescriptors, modelDescriptors, modelIdx, distance, k, mask);
      }

      /// <summary>
      /// Release all the unmanaged memory associated with this matcher
      /// </summary>
      protected override void DisposeObject()
      {
         GpuBruteForceMatcherInvoke.gpuBruteForceMatcherRelease(ref _ptr);
      }
   }

   internal static class GpuBruteForceMatcherInvoke
   {
      [DllImport(CvInvoke.EXTERN_GPU_LIBRARY, CallingConvention = CvInvoke.CvCallingConvention)]
      public extern static IntPtr gpuBruteForceMatcherCreate( DistanceType distType);

      [DllImport(CvInvoke.EXTERN_GPU_LIBRARY, CallingConvention = CvInvoke.CvCallingConvention)]
      public extern static void gpuBruteForceMatcherRelease(ref IntPtr ptr);

      //[DllImport(CvInvoke.EXTERN_GPU_LIBRARY, CallingConvention = CvInvoke.CvCallingConvention)]
      //private extern static void gpuBruteForceMatcherAdd(IntPtr matcher, IntPtr trainDescs);

      [DllImport(CvInvoke.EXTERN_GPU_LIBRARY, CallingConvention = CvInvoke.CvCallingConvention)]
      public extern static void gpuBruteForceMatcherKnnMatch(
         IntPtr matcher,
         IntPtr queryDescs, IntPtr trainDescs,
         IntPtr trainIdx, IntPtr distance,
         int k, IntPtr mask);
   }
}
