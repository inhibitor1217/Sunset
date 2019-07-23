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
    String imageName( "./sc-1.jpg" ); // by default
    Mat image, out;
    image = imread( imageName, IMREAD_COLOR ); // Read the file
    if( image.empty() )                      // Check for invalid input
    {
        cout <<  "Could not open or find the image" << std::endl ;
        return -1;
    }
    
    cvtColor(image, out, COLOR_RGB2Lab);
    
    namedWindow( "Plot", WINDOW_AUTOSIZE ); // Create a window for display.
    imshow( "Plot", out );                // Show our image inside it.
    waitKey(0); // Wait for a keystroke in the window
    return 0;
}

