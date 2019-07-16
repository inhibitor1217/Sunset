#include <opencv2/core.hpp>
#include <opencv2/imgcodecs.hpp>
#include <opencv2/highgui.hpp>

#include <opencv2/ximgproc/slic.hpp>

#include <iostream>
#include <string>

using namespace cv;
using namespace std;

int main( int argc, char** argv )
{
    String imageName( "./image1.jpg" ); // by default
    Mat image;
    image = imread( imageName, IMREAD_COLOR ); // Read the file
    if( image.empty() )                      // Check for invalid input
    {
        cout <<  "Could not open or find the image" << std::endl ;
        return -1;
    }
    Mat out = image, outContour;

    Ptr<ximgproc::SuperpixelSLIC> slic = ximgproc::createSuperpixelSLIC(image, ximgproc::SLICO, 10, 5.0f);
    slic->iterate();
    slic->getLabelContourMask(outContour);
    out.setTo(Scalar(0, 0, 255), outContour);
    int numSuperpixels = slic->getNumberOfSuperpixels();
    cout << numSuperpixels << endl;
    
    namedWindow( "Display window", WINDOW_AUTOSIZE ); // Create a window for display.
    imshow( "Display window", out );                // Show our image inside it.
    waitKey(0); // Wait for a keystroke in the window
    return 0;
}

