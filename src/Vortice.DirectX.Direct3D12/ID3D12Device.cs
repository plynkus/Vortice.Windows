﻿// Copyright (c) Amer Koleci and contributors.
// Distributed under the MIT license. See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using SharpGen.Runtime;
using SharpGen.Runtime.Win32;
using Vortice.DirectX.Direct3D;

namespace Vortice.DirectX.Direct3D12
{
    public partial class ID3D12Device
    {
        private const int GENERIC_ALL = 0x10000000;
        private RootSignatureVersion? _highestRootSignatureVersion;

        public static bool IsSupported(IUnknown adapter, FeatureLevel minFeatureLevel = FeatureLevel.Level_11_0)
        {
            try
            {
                return D3D12.D3D12CreateDeviceNoDevice(adapter, minFeatureLevel).Success;
            }
            catch (DllNotFoundException)
            {
                // On pre Windows 10 d3d12.dll is not present and therefore not supported.
                return false;
            }
        }

        public unsafe RootSignatureVersion HighestRootSignatureVersion
        {
            get
            {
                if (!_highestRootSignatureVersion.HasValue)
                {
                    var featureData = new FeatureDataRootSignature
                    {
                        HighestVersion = RootSignatureVersion.Version11
                    };

                    if (CheckFeatureSupport(Feature.RootSignature, new IntPtr(&featureData), Interop.SizeOf<FeatureDataRootSignature>()).Failure)
                    {
                        _highestRootSignatureVersion = RootSignatureVersion.Version11;
                    }
                    else
                    {
                        _highestRootSignatureVersion = featureData.HighestVersion;
                    }
                }

                return _highestRootSignatureVersion.Value;
            }
        }

        public unsafe T CheckFeatureSupport<T>(Feature feature) where T : struct
        {
            T featureSupport = default;
            CheckFeatureSupport(feature, new IntPtr(Unsafe.AsPointer(ref featureSupport)), Interop.SizeOf<T>());
            return featureSupport;
        }

        public unsafe bool CheckFeatureSupport<T>(Feature feature, ref T featureSupport) where T : struct
        {
            return CheckFeatureSupport(feature, new IntPtr(Unsafe.AsPointer(ref featureSupport)), Interop.SizeOf<T>()).Success;
        }

        public unsafe Result CheckMaxSupportedFeatureLevel(FeatureLevel[] featureLevels, out FeatureLevel maxSupportedFeatureLevel)
        {
            fixed (FeatureLevel* levelsPtr = &featureLevels[0])
            {
                var featureData = new FeatureDataFeatureLevels
                {
                    NumFeatureLevels = featureLevels.Length,
                    PFeatureLevelsRequested = new IntPtr(levelsPtr)
                };

                var result = CheckFeatureSupport(Feature.FeatureLevels, new IntPtr(&featureData), Interop.SizeOf<FeatureDataFeatureLevels>());
                maxSupportedFeatureLevel = featureData.MaxSupportedFeatureLevel;
                return result;
            }
        }

        public unsafe FeatureDataGpuVirtualAddressSupport GpuVirtualAddressSupport
        {
            get
            {
                var featureData = new FeatureDataGpuVirtualAddressSupport();
                if (CheckFeatureSupport(Feature.GpuVirtualAddressSupport, new IntPtr(&featureData), Interop.SizeOf<FeatureDataGpuVirtualAddressSupport>()).Success)
                {
                    return featureData;
                }

                return default;
            }
        }

        public unsafe ShaderModel CheckHighestShaderModel(ShaderModel highestShaderModel)
        {
            var featureData = new FeatureDataShaderModel
            {
                HighestShaderModel = highestShaderModel
            };

            if (CheckFeatureSupport(Feature.ShaderModel, new IntPtr(&featureData), Interop.SizeOf<FeatureDataShaderModel>()).Success)
            {
                return featureData.HighestShaderModel;
            }

            return ShaderModel.Model51;
        }

        public unsafe RootSignatureVersion CheckHighestRootSignatureVersion(RootSignatureVersion highestVersion)
        {
            var featureData = new FeatureDataRootSignature
            {
                HighestVersion = highestVersion
            };

            if (CheckFeatureSupport(Feature.RootSignature, new IntPtr(&featureData), Interop.SizeOf<FeatureDataRootSignature>()).Success)
            {
                return featureData.HighestVersion;
            }

            return RootSignatureVersion.Version10;
        }

        public ID3D12Resource CreateCommittedResource(HeapProperties heapProperties,
            HeapFlags heapFlags,
            ResourceDescription description,
            ResourceStates initialResourceState,
            ClearValue? optimizedClearValue = null)
        {
            return CreateCommittedResource(
                ref heapProperties,
                heapFlags,
                ref description,
                initialResourceState,
                optimizedClearValue,
                typeof(ID3D12Resource).GUID);
        }

        public ID3D12CommandQueue CreateCommandQueue(CommandListType type, int priority = 0, CommandQueueFlags flags = CommandQueueFlags.None, int nodeMask = 0)
        {
            return CreateCommandQueue(new CommandQueueDescription(type, priority, flags, nodeMask), typeof(ID3D12CommandQueue).GUID);
        }

        public ID3D12CommandQueue CreateCommandQueue(CommandListType type, CommandQueuePriority priority, CommandQueueFlags flags = CommandQueueFlags.None, int nodeMask = 0)
        {
            return CreateCommandQueue(new CommandQueueDescription(type, priority, flags, nodeMask), typeof(ID3D12CommandQueue).GUID);
        }

        public ID3D12CommandQueue CreateCommandQueue(CommandQueueDescription description)
        {
            return CreateCommandQueue(description, typeof(ID3D12CommandQueue).GUID);
        }

        public ID3D12DescriptorHeap CreateDescriptorHeap(DescriptorHeapDescription description)
        {
            return CreateDescriptorHeap(description, typeof(ID3D12DescriptorHeap).GUID);
        }

        public ID3D12CommandAllocator CreateCommandAllocator(CommandListType type)
        {
            return CreateCommandAllocator(type, typeof(ID3D12CommandAllocator).GUID);
        }

        public ID3D12GraphicsCommandList CreateCommandList(CommandListType type, ID3D12CommandAllocator commandAllocator, ID3D12PipelineState initialState = null)
        {
            return CreateCommandList(0, type, commandAllocator, initialState);
        }

        public ID3D12GraphicsCommandList CreateCommandList(int nodeMask, CommandListType type, ID3D12CommandAllocator commandAllocator, ID3D12PipelineState initialState = null)
        {
            Guard.NotNull(commandAllocator, nameof(commandAllocator));

            var nativePtr = CreateCommandList(nodeMask, type, commandAllocator, initialState, typeof(ID3D12GraphicsCommandList).GUID);
            return new ID3D12GraphicsCommandList(nativePtr);
        }

        public ID3D12Fence CreateFence(ulong initialValue, FenceFlags flags = FenceFlags.None)
        {
            return CreateFence(initialValue, flags, typeof(ID3D12Fence).GUID);
        }

        public ID3D12Heap CreateHeap(HeapDescription description)
        {
            return CreateHeap(ref description, typeof(ID3D12Heap).GUID);
        }

        public ID3D12RootSignature CreateRootSignature(int nodeMask, RootSignatureDescription description, RootSignatureVersion version)
        {
            var result = D3D12.D3D12SerializeRootSignature(description, version, out var blob, out var errorBlob);
            if (result.Failure)
            {
                if (errorBlob != null)
                {
                    throw new SharpGenException(result, errorBlob.ConvertToString());
                }

                throw new SharpGenException(result);
            }

            try
            {
                return CreateRootSignature(nodeMask, blob.BufferPointer, blob.BufferSize, typeof(ID3D12RootSignature).GUID);
            }
            finally
            {
                blob.Dispose();
            }
        }

        public ID3D12RootSignature CreateRootSignature(RootSignatureDescription description, RootSignatureVersion version)
        {
            return CreateRootSignature(0, description, version);
        }

        public ID3D12RootSignature CreateRootSignature(int nodeMask, VersionedRootSignatureDescription rootSignatureDescription)
        {
            Guard.NotNull(rootSignatureDescription, nameof(rootSignatureDescription));

            var result = Result.Ok;
            Blob signature = null;
            Blob errorBlob = null;

            // D3DX12SerializeVersionedRootSignature
            switch (HighestRootSignatureVersion)
            {
                case RootSignatureVersion.Version10:
                    switch (rootSignatureDescription.Version)
                    {
                        case RootSignatureVersion.Version10:
                            result = D3D12.D3D12SerializeRootSignature(rootSignatureDescription.Description_1_0, RootSignatureVersion.Version1, out signature, out errorBlob);
                            break;

                        case RootSignatureVersion.Version11:
                            // Convert to version 1.0.
                            var desc_1_1 = rootSignatureDescription.Description_1_1;
                            RootParameter[] parameters_1_0 = null;

                            if (desc_1_1.Parameters?.Length > 0)
                            {
                                parameters_1_0 = new RootParameter[desc_1_1.Parameters.Length];
                                for (var i = 0; i < parameters_1_0.Length; i++)
                                {
                                    parameters_1_0[i].ParameterType = desc_1_1.Parameters[i].ParameterType;
                                    parameters_1_0[i].ShaderVisibility = desc_1_1.Parameters[i].ShaderVisibility;

                                    switch (desc_1_1.Parameters[i].ParameterType)
                                    {
                                        case RootParameterType.Constant32Bits:
                                            parameters_1_0[i].Constants = desc_1_1.Parameters[i].Constants;
                                            break;

                                        case RootParameterType.ConstantBufferView:
                                        case RootParameterType.ShaderResourceView:
                                        case RootParameterType.UnorderedAccessView:
                                            parameters_1_0[i].Descriptor = new RootDescriptor(
                                                desc_1_1.Parameters[i].Descriptor.ShaderRegister,
                                                desc_1_1.Parameters[i].Descriptor.RegisterSpace);
                                            break;

                                        case RootParameterType.DescriptorTable:
                                            var table_1_1 = desc_1_1.Parameters[i].DescriptorTable;
                                            var ranges = new DescriptorRange[table_1_1.Ranges?.Length ?? 0];

                                            for (var x = 0; x < ranges.Length; x++)
                                            {
                                                ranges[x].BaseShaderRegister = table_1_1.Ranges[x].BaseShaderRegister;
                                                ranges[x].NumDescriptors = table_1_1.Ranges[x].NumDescriptors;
                                                ranges[x].OffsetInDescriptorsFromTableStart = table_1_1.Ranges[x].OffsetInDescriptorsFromTableStart;
                                                ranges[x].RangeType = table_1_1.Ranges[x].RangeType;
                                                ranges[x].RegisterSpace = table_1_1.Ranges[x].RegisterSpace;
                                            }

                                            parameters_1_0[i].DescriptorTable = new RootDescriptorTable(ranges);
                                            break;
                                    }
                                }
                            }

                            var desc_1_0 = new RootSignatureDescription(desc_1_1.Flags, parameters_1_0, desc_1_1.StaticSamplers);
                            result = D3D12.D3D12SerializeRootSignature(desc_1_0, RootSignatureVersion.Version1, out signature, out errorBlob);
                            break;
                    }
                    break;

                case RootSignatureVersion.Version11:
                    result = D3D12.D3D12SerializeVersionedRootSignature(rootSignatureDescription, out signature, out errorBlob);
                    break;
            }

            if (result.Failure || signature == null)
            {
                if (errorBlob != null)
                {
                    throw new SharpGenException(result, errorBlob.ConvertToString());
                }

                throw new SharpGenException(result);
            }

            try
            {
                return CreateRootSignature(nodeMask, signature.BufferPointer, signature.BufferSize, typeof(ID3D12RootSignature).GUID);
            }
            finally
            {
                signature.Dispose();
            }
        }

        public ID3D12RootSignature CreateRootSignature(VersionedRootSignatureDescription rootSignatureDescription)
        {
            return CreateRootSignature(0, rootSignatureDescription);
        }

        public ID3D12CommandSignature CreateCommandSignature(CommandSignatureDescription description, ID3D12RootSignature rootSignature)
        {
            Guard.NotNull(rootSignature, nameof(rootSignature));

            return CreateCommandSignature(ref description, rootSignature, typeof(ID3D12CommandSignature).GUID);
        }

        public ID3D12PipelineState CreateComputePipelineState(ComputePipelineStateDescription description)
        {
            Guard.NotNull(description, nameof(description));

            return CreateComputePipelineState(description, typeof(ID3D12PipelineState).GUID);
        }

        public ID3D12PipelineState CreateGraphicsPipelineState(GraphicsPipelineStateDescription description)
        {
            Guard.NotNull(description, nameof(description));

            return CreateGraphicsPipelineState(description, typeof(ID3D12PipelineState).GUID);
        }

        public ID3D12QueryHeap CreateQueryHeap(QueryHeapDescription description)
        {
            return CreateQueryHeap(description, typeof(ID3D12QueryHeap).GUID);
        }

        public ID3D12Resource CreatePlacedResource(
            ID3D12Heap heap,
            ulong heapOffset,
            ResourceDescription resourceDescription,
            ResourceStates initialState,
            ClearValue? clearValue = null)
        {
            Guard.NotNull(heap, nameof(heap));

            return CreatePlacedResource(heap, heapOffset, ref resourceDescription, initialState, clearValue, typeof(ID3D12Resource).GUID);
        }

        public ID3D12Resource CreateReservedResource(ResourceDescription resourceDescription, ResourceStates initialState, ClearValue? clearValue = null)
        {
            return CreateReservedResource(ref resourceDescription, initialState, clearValue, typeof(ID3D12Resource).GUID);
        }

        public IntPtr CreateSharedHandle(ID3D12DeviceChild deviceChild, SecurityAttributes? attributes, string name)
        {
            Guard.NotNull(deviceChild, nameof(deviceChild));
            Guard.NotNullOrEmpty(name, nameof(name));

            return CreateSharedHandlePrivate(deviceChild, attributes, GENERIC_ALL, name);
        }

        /// <summary>
        /// Opens a handle for shared resources, shared heaps, and shared fences.
        /// </summary>
        /// <typeparam name="T">The handle that was output by the call to <see cref="CreateSharedHandle(ID3D12DeviceChild, SecurityAttributes?, string)"/> </typeparam>
        /// <param name="handle"></param>
        /// <returns>Instance of <see cref="ID3D12Heap"/>, <see cref="ID3D12Resource"/> or <see cref="ID3D12Fence"/>.</returns>
        public T OpenSharedHandle<T>(IntPtr handle) where T : ComObject
        {
            Guard.IsTrue(handle != IntPtr.Zero, nameof(handle), "Invalid handle");
            var result = OpenSharedHandle(handle, typeof(T).GUID, out var nativePtr);
            if (result.Failure)
            {
                return default;
            }

            return FromPointer<T>(nativePtr);
        }

        /// <summary>
        /// Opens a handle for shared resources, shared heaps, and shared fences, by using Name and Access.
        /// </summary>
        /// <param name="name">The name that was optionally passed as the name parameter in the call to <see cref="CreateSharedHandle(ID3D12DeviceChild, SecurityAttributes?, string)"/> </param>
        /// <returns>The shared handle.</returns>
        public IntPtr OpenSharedHandleByName(string name)
        {
            var result = OpenSharedHandleByName(name, GENERIC_ALL, out var handleRef);
            if (result.Failure)
            {
                return IntPtr.Zero;
            }

            return handleRef;
        }

        public HeapProperties GetCustomHeapProperties(HeapType heapType)
        {
            return GetCustomHeapProperties(0, heapType);
        }

        public void Evict(params ID3D12Pageable[] objects)
        {
            Evict(objects.Length, objects);
        }

        public ResourceAllocationInfo GetResourceAllocationInfo(int visibleMask, params ResourceDescription[] resourceDescriptions)
        {
            return GetResourceAllocationInfo(visibleMask, resourceDescriptions.Length, resourceDescriptions);
        }

        public ResourceAllocationInfo GetResourceAllocationInfo(params ResourceDescription[] resourceDescriptions)
        {
            return GetResourceAllocationInfo(0, resourceDescriptions.Length, resourceDescriptions);
        }
    }
}
