#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>

#include <vector>

using namespace cv;
using namespace std;

extern "C"
{
    void processPCA(uchar *inImageArray, uchar *inMaskArray, uchar *inLabelArray, int width, int height, int numSegments, uchar *outPaletteArray);
}

const float VALID_RATIO_THRESHOLD = .5f;

void convertToNormalizedLab(const Mat &img, Mat *channels);
void convertToRGB(const Mat& src, const Mat& dst);
void decodeLabel(const Mat& labelEncoded, Mat &label);
int computeStats(Mat *channels, const Mat &mask, const Mat &label, vector<Vec3f> &average, vector<int> &weights);
void pca(Mat &X, const vector<int> &weights, Vec3f &center, Vec3f &fpc, vector<float> &fpc_components);
void computePalette(const vector<float> &quartiles, const vector<float> &fpc_components, Vec3f center, Vec3f fpc, Mat &palette);

void processPCA(uchar *inImageArray, uchar *inMaskArray, uchar *inLabelArray, int width, int height, int numSegments, uchar *outPaletteArray)
{
    int num_valid_segments;
    Mat img, mask, labelEncoded, img_channels[3], label, X, palette;
    Vec3f center, fpc;
    vector<int>   weights;
    vector<float> fpc_components, quartiles { .03f, .1f, .2f, .3f, .4f, .5f, .6f, .7f, .8f, .9f, .97f };
    vector<Vec3f> average;
    
    img = Mat(height, width, CV_8UC4, inImageArray);
    mask = Mat(height, width, CV_8UC1, inMaskArray);
    labelEncoded = Mat(height, width, CV_8UC3, inLabelArray);
    
    decodeLabel(labelEncoded, label);
    convertToNormalizedLab(img, img_channels);
    
    num_valid_segments = computeStats(img_channels, img_channels[0], label, average, weights);
    
    X = Mat(num_valid_segments, 3, CV_32FC1);
    for (int i = 0; i < num_valid_segments; i++)
    {
        X.at<float>(i, 0) = average[i][0];
        X.at<float>(i, 1) = average[i][1];
        X.at<float>(i, 2) = average[i][2];
    }
    pca(X, weights, center, fpc, fpc_components);
    
    computePalette(quartiles, fpc_components, center, fpc, palette);
    
    convertToRGB(palette, palette);
    
    if (outPaletteArray)
        std::memcpy(outPaletteArray, palette.data, palette.total() * palette.elemSize());
    
    img.release();
    mask.release();
    labelEncoded.release();
    img_channels[0].release();
    img_channels[1].release();
    img_channels[2].release();
    label.release();
    X.release();
    palette.release();
}

void convertToNormalizedLab(const Mat &img, Mat *channels)
{
    Mat temp;
    
    cv::cvtColor(img, temp, cv::COLOR_BGR2Lab);
    temp.convertTo(temp, CV_32FC3);
    cv::split(temp, channels);
    channels[0] *= 100.0f/255.0f;
    channels[1] -= 128.0f;
    channels[2] -= 128.0f;
    
    temp.release();
}

void convertToRGB(const Mat& src, const Mat& dst)
{
    cvtColor(src, dst, cv::COLOR_Lab2BGR);
}

void decodeLabel(const Mat& labelEncoded, Mat &label)
{
    Mat channels[3];
    
    split(labelEncoded, channels);
    channels[2].convertTo(label, CV_32SC1);
    label *= 256;
    add(label, channels[1], label, noArray(), CV_32SC1);
    label *= 256;
    add(label, channels[0], label, noArray(), CV_32SC1);
    
    channels[0].release();
    channels[1].release();
    channels[2].release();
}

int computeStats(Mat *channels, const Mat &mask, const Mat &label, vector<Vec3f> &average, vector<int> &weights)
{
    int num_segments, num_valid_segments;
    double num_segments_double;
    Mat seg_sum;
    vector<int> count, valid_count, valid_segments;
    
    minMaxLoc(label, NULL, &num_segments_double);
    num_segments = (int)num_segments_double;
    
    seg_sum = Mat(num_segments, 3, CV_32FC1);
    count = vector<int>(num_segments);
    valid_count = vector<int>(num_segments);
    
    for (int y = 0; y < label.rows; y++)
    {
        for (int x = 0; x < label.cols; x++)
        {
            int cur_label = label.at<int>(y, x);
            if (mask.at<float>(y, x) > 1.0f)
            {
                seg_sum.at<float>(cur_label, 0) += channels[0].at<float>(y, x);
                seg_sum.at<float>(cur_label, 1) += channels[1].at<float>(y, x);
                seg_sum.at<float>(cur_label, 2) += channels[2].at<float>(y, x);
                valid_count[cur_label]++;
            }
            count[cur_label]++;
        }
    }
    
    for (int i = 0; i < num_segments; i++)
    {
        if ((float)valid_count[i] / (float)count[i] > VALID_RATIO_THRESHOLD)
        {
            valid_segments.push_back(i);
            weights.push_back(valid_count[i]);
        }
    }
    num_valid_segments = (int)valid_segments.size();
    
    for (int i = 0; i < num_valid_segments; i++)
    {
        average.push_back(Vec3f(seg_sum.at<float>(valid_segments[i], 0),
                                seg_sum.at<float>(valid_segments[i], 1),
                                seg_sum.at<float>(valid_segments[i], 2)) / (float)weights[i]);
    }
    
    seg_sum.release();
    
    return num_valid_segments;
}

void pca(Mat &X, const vector<int> &weights, Vec3f &center, Vec3f &fpc, vector<float> &fpc_components)
{
    int total_weights = 0, num_valid_segments = (int)weights.size();
    Mat X_prime, U, S, Vt, X_V;
    
    center = Vec3f(0, 0, 0);
    
    for (int i = 0; i < num_valid_segments; i++)
    {
        center[0] += X.at<float>(i, 0) * weights[i];
        center[1] += X.at<float>(i, 1) * weights[i];
        center[2] += X.at<float>(i, 2) * weights[i];
        
        total_weights += weights[i];
    }
    center /= (float)total_weights;
    
    for (int i = 0; i < num_valid_segments; i++)
    {
        X.at<float>(i, 0) -= center[0];
        X.at<float>(i, 1) -= center[1];
        X.at<float>(i, 2) -= center[2];
    }
    X.copyTo(X_prime);
    for (int i = 0; i < num_valid_segments; i++)
    {
        X_prime.at<float>(i, 0) *= sqrt(weights[i]);
        X_prime.at<float>(i, 1) *= sqrt(weights[i]);
        X_prime.at<float>(i, 2) *= sqrt(weights[i]);
    }
    
    SVD::compute(X_prime, U, S, Vt);
    if (Vt.at<float>(0, 0) < 0)
        Vt = -Vt;
    
    fpc = Vec3f(Vt.at<float>(0, 0), Vt.at<float>(0, 1), Vt.at<float>(0, 2));
    
    X_V = X * Vt.t();
    
    fpc_components = vector<float>(num_valid_segments);
    for (int i = 0; i < num_valid_segments; i++)
    {
        fpc_components[i] = X_V.at<float>(i, 0);
    }
    sort(fpc_components.begin(), fpc_components.end());
    
    X_prime.release();
    U.release();
    S.release();
    Vt.release();
    X_V.release();
}

void computePalette(const vector<float> &quartiles, const vector<float> &fpc_components, Vec3f center, Vec3f fpc, Mat &palette)
{
    int num_valid_segments = (int)fpc_components.size();
    vector<float> scaled_quartiles((int)quartiles.size());
    
    palette = Mat((int)quartiles.size(), 1, CV_32FC3);
    
    for (int i = 0; i < (int)quartiles.size(); i++)
    {
        scaled_quartiles[i] = quartiles[i] * (float)num_valid_segments;
    }
    
    for (int i = 0; i < (int)quartiles.size(); i++)
    {
        int low  = (int)floor(scaled_quartiles[i]);
        int high = (int)ceil (scaled_quartiles[i]);
        float t  = scaled_quartiles[i] - (float)low;
        
        palette.at<cv::Vec3f>(i, 0) = center + fpc * ( fpc_components[low] * (1.0f - t) + fpc_components[high] * t );
    }
}
