#include <opencv2/ximgproc/slic.hpp>
#include <opencv2/imgproc.hpp>

using namespace cv;

extern "C"
{
    int processSLIC(uchar *inputArray, int width, int height, int *outputLabelArray, uchar *outputContourArray, int algorithm, int region_size, float ruler);
}
    
int processSLIC(uchar *inputArray, int width, int height, int *outputLabelArray, uchar *outputContourArray, int algorithm=ximgproc::SLICO, int region_size=10, float ruler=10.0f)
{
    Mat input = Mat(height, width, CV_8UC4, inputArray), outputLabel, outputLabelContour;
    
    // Preprocess image
    GaussianBlur(input, input, Size(3, 3), 0.8);
    cvtColor(input, input, COLOR_BGR2Lab);
    
    // Process image through SLIC
    Ptr<ximgproc::SuperpixelSLIC> slic = ximgproc::createSuperpixelSLIC(input, algorithm, region_size, ruler);

    slic->iterate();
    if (outputLabelArray)
        slic->getLabels(outputLabel);
    if (outputContourArray)
        slic->getLabelContourMask(outputLabelContour, false);
    int numSuperpixels = slic->getNumberOfSuperpixels();
    
    // Coppy data to output buffer
    if (outputLabelArray)
        std::memcpy(outputLabelArray, outputLabel.data, outputLabel.total() * outputLabel.elemSize());
    if (outputContourArray)
        std::memcpy(outputContourArray, outputLabelContour.data, outputLabelContour.total() * outputLabelContour.elemSize());
    
    return numSuperpixels;
}
