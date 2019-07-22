#include <opencv2/core.hpp>
#include <opencv2/imgcodecs.hpp>
#include <opencv2/highgui.hpp>

#include <opencv2/imgproc.hpp>
#include <opencv2/ximgproc/slic.hpp>

#include <iostream>
#include <string>

using namespace cv;
using namespace std;

int main( int argc, char** argv )
{
    String imageName( "./beach-4.jpg" ); // by default
    Mat image;
    image = imread( imageName, IMREAD_COLOR ); // Read the file
    if( image.empty() )                      // Check for invalid input
    {
        cout <<  "Could not open or find the image" << std::endl ;
        return -1;
    }
    Mat out = image, outContour;
    
    // Preprocess image
    GaussianBlur(image, image, Size(3, 3), 0.8);
    cvtColor(image, image, COLOR_BGR2Lab);

    Ptr<ximgproc::SuperpixelSLIC> slic = ximgproc::createSuperpixelSLIC(image, ximgproc::SLIC, 256, 1e10);
    slic->iterate(10);
    slic->getLabelContourMask(outContour);
    out.setTo(Scalar(0, 0, 255), outContour);
    int numSuperpixels = slic->getNumberOfSuperpixels();
    cout << numSuperpixels << endl;
    
    namedWindow( "Display window", WINDOW_AUTOSIZE ); // Create a window for display.
    imshow( "Display window", out );                // Show our image inside it.
    waitKey(0); // Wait for a keystroke in the window
    return 0;
}

