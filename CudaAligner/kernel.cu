#include "cuda_runtime.h"
#include "device_launch_parameters.h"
#include <thrust/host_vector.h>
#include <thrust/device_vector.h>
#include <thrust/generate.h>
#include <thrust/sort.h>
#include <thrust/copy.h>
#include <algorithm>
#include <cstdlib>
#include <chrono>

#include <stdio.h>

cudaError_t addWithCuda(int* c, const int* a, const int* b, unsigned int size);

__global__ void addKernel(int* c, const int* a, const int* b)
{
	int i = threadIdx.x;
	c[i] = a[i] + b[i];
}

// note: functor inherits from unary_function
struct get_binaryIndex : public thrust::unary_function<unsigned int, unsigned int>
{
	__host__ __device__
		unsigned int operator()(unsigned int x) const
	{
		return x & 0xFFFF0000;
	}
};

// note: functor inherits from unary_function
struct get_setFlags : public thrust::unary_function<unsigned int, unsigned int>
{
	__host__ __device__
		unsigned int operator()(unsigned int x) const
	{
		return x & 0x0000FFFF;
	}
};

/*struct Compare_custom
{
	bool operator () (const Example& first, const Example& second)
	{
		if (first.a.size() > second.a.size())
			return true;
		else
			return false;
	}
};
*/
int main()
{
	int deviceCount;
	cudaGetDeviceCount(&deviceCount);
	for (int i = 0; i < deviceCount; i++) {
		cudaDeviceProp deviceProp;
		cudaGetDeviceProperties(&deviceProp, i);
		std::cout << "Device: " << deviceProp.computeMode << " " << deviceProp.name << std::endl;
	}

	cudaSetDevice(0);
	// generate 32M random numbers serially
	long long vectorSize = 600000000;
	thrust::host_vector<unsigned int> h_vec1(vectorSize);
	thrust::host_vector<unsigned int> h_vec2(vectorSize);
	thrust::host_vector<unsigned int> h_vec3(vectorSize);
	std::generate(h_vec1.begin(), h_vec1.end(), rand);
	std::generate(h_vec2.begin(), h_vec2.end(), rand);

	auto start = std::chrono::high_resolution_clock::now();
	// transfer data to the device
	thrust::device_vector<int> d_vec1 = h_vec1;
	thrust::device_vector<int> d_vec2 = h_vec2;

	// transfer data back to host
	thrust::copy(d_vec1.begin(), d_vec1.end(), h_vec1.begin());
	auto stop = std::chrono::high_resolution_clock::now();

	auto duration = std::chrono::duration_cast<std::chrono::seconds>(stop - start);

	// To get the value of duration use the count()
	// member function on the duration object
	std::cout << "Execution time in seconds: " << duration.count() << std::endl;

	return 0;
}