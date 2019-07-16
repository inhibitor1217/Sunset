#include <opencv2/ximgproc/slic.hpp>
#include <opencv2/highgui.hpp>

using namespace cv;

extern "C"
{
    int processSLIC(uchar *inputArray, int width, int height, int *outputLabelArray, uchar *outputContourArray, int algorithm, int region_size, float ruler);
}
    
int processSLIC(uchar *inputArray, int width, int height, int *outputLabelArray, uchar *outputContourArray, int algorithm=ximgproc::SLICO, int region_size=10, float ruler=10.0f)
{
    Mat input = Mat(height, width, CV_8UC4, inputArray), outputLabel, outputLabelContour;
    
    Ptr<ximgproc::SuperpixelSLIC> slic = ximgproc::createSuperpixelSLIC(input, algorithm, region_size, ruler);

    slic->iterate();
    slic->getLabels(outputLabel);
    slic->getLabelContourMask(outputLabelContour, false);
    int numSuperpixels = slic->getNumberOfSuperpixels();
    
    std::memcpy(outputLabelArray, outputLabel.data, outputLabel.total() * outputLabel.elemSize());
    std::memcpy(outputContourArray, outputLabelContour.data, outputLabelContour.total() * outputLabelContour.elemSize());
    
    return numSuperpixels;
}
