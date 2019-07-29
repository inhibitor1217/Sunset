#include <opencv2/core.hpp>
#include <opencv2/imgproc.hpp>

#include <vector>
#include <algorithm>
#include <map>

using namespace cv;
using namespace std;

const float PERFORM_PCA_THRESHOLD = 50;

extern "C"
{
    void processOctavePCA(uchar *inImageArray, uchar *inMaskArray, uchar *inLabelArray, int width, int height, int levels, float **outLowArray, float **outHighArray);
}

extern void convertToNormalizedLab(const Mat &img, Mat *channels);
extern void convertToRGB(const Mat& src, const Mat& dst);
extern void decodeLabel(const Mat& labelEncoded, Mat &label);

map<int, pair<Vec3f, float>> *pca_octave_rec(Mat *channels, const Mat &mask, const Mat &label, Mat *low, Mat *high, int level, int x, int y, int width, int height, int iX, int iY);
void pca_octave(Mat *channels, const Mat &mask, const Mat &label, int levels, Mat *low, Mat *high);

void processOctavePCA(uchar *inImageArray, uchar *inMaskArray, uchar *inLabelArray, int width, int height, int levels, float **outLowArray, float **outHighArray)
{
    Mat img, mask, labelEncoded, img_channels[3], label, low[levels], high[levels];
    
    img          = Mat(height, width, CV_8UC4, inImageArray);
    mask         = Mat(height, width, CV_8UC1, inMaskArray);
    labelEncoded = Mat(height, width, CV_8UC3, inLabelArray);
    
    decodeLabel(labelEncoded, label);
    convertToNormalizedLab(img, img_channels);
    
    pca_octave(img_channels, mask, label, levels, low, high);
    
    for (int level = 0; level < levels; level++)
    {
        if (outLowArray)
            memcpy(outLowArray[level], low[level].data, low[level].total() * low[level].elemSize());
        if (outHighArray)
            memcpy(outHighArray[level], high[level].data, high[level].total() * high[level].elemSize());
    }
    
    img.release();
    mask.release();
    labelEncoded.release();
    img_channels[0].release();
    img_channels[1].release();
    img_channels[2].release();
    label.release();
    
    for (int level = 0; level < levels; level++)
    {
        low [level].release();
        high[level].release();
    }
}

map<int, pair<Vec3f, float>> *pca_octave_rec(Mat *channels, const Mat &mask, const Mat &label, Mat *low, Mat *high, int level, int x, int y, int width, int height, int iX, int iY)
{
    if (level < 0)
        return nullptr;
    
    auto ret = new map<int, pair<Vec3f, float> >();
    
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

void pca_octave(Mat *channels, const Mat &mask, const Mat &label, int levels, Mat *low, Mat *high)
{
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
