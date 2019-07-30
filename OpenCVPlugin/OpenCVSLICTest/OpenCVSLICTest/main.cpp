#include <opencv2/core.hpp>
#include <opencv2/imgcodecs.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/highgui.hpp>

#include <iostream>
#include <string>
#include <algorithm>
#include <vector>
#include <map>

using namespace cv;
using namespace std;

const float VALID_RATIO_THRESHOLD = .5f;
const float WINDOW_SIZE_THRESHOLD = 128*128;
const float PERFORM_PCA_THRESHOLD = 50;

void convertToNormalizedLab(const Mat &img, Mat *channels);
void convertToRGB(const Mat& src, const Mat& dst);
void decodeLabel(const Mat& labelEncoded, Mat &label);
int computeStats(Mat *channels, const Mat &mask, const Mat &label, vector<Vec3f> &average, vector<int> &weights);
void pca(Mat &X, const vector<int> &weights, Vec3f &center, Vec3f &fpc, vector<float> &fpc_components);
void computePalette(const vector<float> &quartiles, const vector<float> &fpc_components, Vec3f center, Vec3f fpc, Mat &palette);

map<int, pair<Vec3f, float>> *pca_octave_rec(Mat *channels, const Mat &mask, const Mat &label, Mat *low, Mat *high, int level, int x, int y, int width, int height, int iX, int iY)
{
    if (level < 0)
        return nullptr;
    
    auto *ret = new map<int, pair<Vec3f, float> >();
    
    if (level == 0)
    {
        for (int _y = y; _y < y + height; _y++)
            for (int _x = x; _x < x + width; _x++)
                if (mask.at<uchar>(_y, _x) > 0)
                {
                    int cur_label = label.at<int>(_y, _x);
                    auto it = ret->find(cur_label);
                    
                    if (it == ret->end())
                    {
                        auto _r = ret->insert(make_pair(cur_label, make_pair(Vec3f(0, 0, 0), 0)));
                        it = _r.first;
                    }
                    
                    (it->second).first[0] += channels[0].at<float>(_y, _x);
                    (it->second).first[1] += channels[1].at<float>(_y, _x);
                    (it->second).first[2] += channels[2].at<float>(_y, _x);
                    (it->second).second   += 1.0f;
                }
    }
    else
    {
        auto r1 = pca_octave_rec(channels, mask, label, low, high, level-1, x, y, width/2, height/2, 2*iX, 2*iY);
        auto r2 = pca_octave_rec(channels, mask, label, low, high, level-1, x + width/2, y, (width+1)/2, height/2, 2*iX+1, 2*iY);
        auto r3 = pca_octave_rec(channels, mask, label, low, high, level-1, x, y + height/2, width/2, (height+1)/2, 2*iX, 2*iY+1);
        auto r4 = pca_octave_rec(channels, mask, label, low, high, level-1, x + width/2, y + height/2, (width+1)/2, (height+1)/2, 2*iX+1, 2*iY+1);
        
        for (auto it = r1->begin(); it != r1->end(); it++)
        {
            auto _it = ret->find(it->first);
            if (_it == ret->end())
            {
                auto _r = ret->insert(make_pair(it->first, make_pair(Vec3f(0, 0, 0), 0)));
                _it = _r.first;
            }
            
            (_it->second).first  += (it->second).first;
            (_it->second).second += (it->second).second;
        }
        
        for (auto it = r2->begin(); it != r2->end(); it++)
        {
            auto _it = ret->find(it->first);
            if (_it == ret->end())
            {
                auto _r = ret->insert(make_pair(it->first, make_pair(Vec3f(0, 0, 0), 0)));
                _it = _r.first;
            }
            
            (_it->second).first  += (it->second).first;
            (_it->second).second += (it->second).second;
        }
        
        for (auto it = r3->begin(); it != r3->end(); it++)
        {
            auto _it = ret->find(it->first);
            if (_it == ret->end())
            {
                auto _r = ret->insert(make_pair(it->first, make_pair(Vec3f(0, 0, 0), 0)));
                _it = _r.first;
            }
            
            (_it->second).first  += (it->second).first;
            (_it->second).second += (it->second).second;
        }
        
        for (auto it = r4->begin(); it != r4->end(); it++)
        {
            auto _it = ret->find(it->first);
            if (_it == ret->end())
            {
                auto _r = ret->insert(make_pair(it->first, make_pair(Vec3f(0, 0, 0), 0)));
                _it = _r.first;
            }
            
            (_it->second).first  += (it->second).first;
            (_it->second).second += (it->second).second;
        }
        
        r1->clear();
        r2->clear();
        r3->clear();
        r4->clear();
        
        free(r1);
        free(r2);
        free(r3);
        free(r4);
    }
    
    Mat X, X_prime, X_V, U, S, Vt;
    Vec3f center = Vec3f(0, 0, 0), fpc;
    float weight_sum = 0.0f;
    
    if (ret->size() >= PERFORM_PCA_THRESHOLD)
    {
        for (auto it = ret->begin(); it != ret->end(); it++)
        {
            center += (it->second).first;
            weight_sum += (it->second).second;
        }
        
        if (weight_sum > 0)
        {
            center /= weight_sum;
            X       = Mat((int)ret->size(), 3, CV_32FC1);
            X_prime = Mat((int)ret->size(), 3, CV_32FC1);
            
            int i = 0;
            for (auto it = ret->begin(); it != ret->end(); it++, i++)
            {
                Vec3f x_i = (it->second).first / (it->second).second - center;
                X.at<float>(i, 0) = x_i[0];
                X.at<float>(i, 1) = x_i[1];
                X.at<float>(i, 2) = x_i[2];
                
                Vec3f x_prime_i = x_i * sqrt((it->second).second);
                X_prime.at<float>(i, 0) = x_prime_i[0];
                X_prime.at<float>(i, 1) = x_prime_i[1];
                X_prime.at<float>(i, 2) = x_prime_i[2];
            }
            
            SVD::compute(X_prime, U, S, Vt);
            if (Vt.at<float>(0, 0) < 0)
                Vt = -Vt;
            
            fpc = Vec3f(Vt.at<float>(0, 0), Vt.at<float>(0, 1), Vt.at<float>(0, 2));
            
            X_V = X * Vt.t();
            vector<float> X_e1 = vector<float>((int)ret->size());
            for (int i = 0; i < (int)ret->size(); i++)
            {
                X_e1[i] = X_V.at<float>(i, 0);
            }
            sort(X_e1.begin(), X_e1.end());
            
            low [level].at<Vec3f>(iY, iX) = center + fpc * X_e1[ round(.05 * (float)ret->size()) ];
            high[level].at<Vec3f>(iY, iX) = center + fpc * X_e1[ round(.95 * (float)ret->size()) ];
            
            X.release();
            X_prime.release();
            X_V.release();
            U.release();
            S.release();
            Vt.release();
        }
    }
    else
    {
        low [level].at<Vec3f>(iY, iX) = Vec3f(0, 0, 0);
        high[level].at<Vec3f>(iY, iX) = Vec3f(0, 0, 0);
    }
    
    return ret;
}

void pca_octave(Mat *channels, const Mat &mask, const Mat &label, Mat *&low, Mat *&high, int &levels)
{
    int _width  = label.cols;
    int _height = label.rows;
    
    levels = 0;
    while (_width * _height >= WINDOW_SIZE_THRESHOLD)
    {
        levels++; _width >>= 1; _height >>= 1;
    }
    
    low  = (Mat *) malloc(sizeof(Mat) * levels);
    high = (Mat *) malloc(sizeof(Mat) * levels);
    
    for (int level = 0; level < levels; level++)
    {
        low [level] = Mat(1 << (levels - 1 - level), 1 << (levels - 1 - level), CV_32FC3);
        high[level] = Mat(1 << (levels - 1 - level), 1 << (levels - 1 - level), CV_32FC3);
    }
    
    pca_octave_rec(channels, mask, label, low, high, levels - 1, 0, 0, label.cols, label.rows, 0, 0);
    
    for (int level = 0; level < levels; level++)
    {
        convertToRGB(low[level],  low[level] );
        convertToRGB(high[level], high[level]);
    }
    
}

void processPCA(const Mat &img, const Mat &labelEncoded, Mat &palette)
{
    int num_valid_segments;
    Mat img_channels[3], label, X;
    Vec3f center, fpc;
    vector<int>   weights;
    vector<float> fpc_components, quartiles { .03f, .1f, .2f, .3f, .4f, .5f, .6f, .7f, .8f, .9f, .97f };
    vector<Vec3f> average;
    
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
    
    img_channels[0].release();
    img_channels[1].release();
    img_channels[2].release();
    label.release();
    X.release();
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
    cv::cvtColor(src, dst, cv::COLOR_Lab2BGR);
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
    
    cout << "CENTER: " << center << endl;
    cout << "FPC:    " << fpc   << endl;
    
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

int main( int argc, char** argv )
{
    Mat img, label_raw, palette, img_channels[3], *low, *high;
    
    img = imread("sf-5-mask.png");
    label_raw = imread("sf-5-slic.png");
    
    split(img, img_channels);
    
//    processPCA(img, label_raw, palette);
    
    int levels;
    Mat label, img_channels_lab[3];
    
    decodeLabel(label_raw, label);
    convertToNormalizedLab(img, img_channels_lab);
    
    pca_octave(img_channels_lab, img_channels[0], label, low, high, levels);
    
//    for (int i = levels - 1; i >= 0; i--)
//    {
//        imshow("COLORS", low[i]);
//        waitKey(0);
//        imshow("COLORS", high[i]);
//        waitKey(0);
//    }
    
    for (int i = levels - 1; i >= 0; i--)
    {
        cout << low[i].total() * low[i].elemSize() / sizeof(float) << endl;
        cout << high[i].total() * high[i].elemSize() / sizeof(float) << endl;
    }
    
    free(low);
    free(high);
    
//    cv::namedWindow("COLORS");
//    cv::imshow("COLORS", palette);
//    cv::waitKey(0);
    
    return 0;
}

