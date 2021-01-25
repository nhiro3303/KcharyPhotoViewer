#pragma once

#include <iostream>
#include <opencv2/opencv.hpp>
#include <libraw/libraw_types.h>
#include "IImageController.h"

using namespace std;

namespace Kchary::ImageController::RawImageControl
{
	class RawImageController : public IImageController
	{
	public:
        /**
		 * @brief	This function is getting image data using libraw library.
		 *			Get from a thumbnail image that can be read at high speed for display.
		 *
		 * @param	const char* path: Raw image file path.
		 * @param	uint8_t** buffer: byte buffer data (out)
		 * @param	unsigned int* size: buffer size (out)
		 * @param   int* stride: Stride data (out)
		 * @param   int* width: Image width (out)
		 * @param   int* height: Image height (out)
		 *
		 * @return	Success: 0, Failure: 1
		 */
		int GetImageData(const char* path, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height) override;

		/**
		 * @brief	This function is getting thumbnail image data.
		 * 
		 * @param	const char* path: Raw image file path.
		 * @param	int resizeLongSideLength: Long side length of a resize image.
		 * @param	uint8_t** buffer: byte buffer data (out)
		 * @param	unsigned int* size: buffer size (out)
		 * @param   int* stride: Stride data (out)
		 * @param   int* width: Image width (out)
		 * @param   int* height: Image height (out)
		 * 
		 * @return	Success: 0, Failure: 1
		 */
		int GetThumbnailImageData(const char* path, int resizeLongSideLength, uint8_t** buffer, unsigned int* size, int* stride, int* width, int* height) override;

	private:
		cv::ImreadModes GetImreadMode(libraw_thumbnail_t thumbnail, int resizeLongSideLength);
	};
}