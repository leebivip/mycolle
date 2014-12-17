
// primes.cpp by wiki908
//

#include <stdio.h>
#include <math.h>

int main()
{
//start:

    int max = 0;
    printf("input number:");
    scanf("%d", &max);

    int len = (max - 2) / 2 + 1;
    //max /= 2;
    
    // 建立完整数据表
    int* tb = new int[len];
    memset(tb, 1, len);
    

    // 去掉所有合数
    // 3 == 0 * 2 + 3;
    // 5 == 1 * 2 + 3;
    int lim = sqrt(max);
    for (int i = 3; i <= lim; i += 2)
    {
        if (tb[(i - 3) / 2])
        {
            for (int j = 3 * i; j <= max; j += (i * 2))
            {
                tb[(j - 3) / 2] = 0;
            }
        }
    }

    // 打印结果
    puts("done\n");
    //for (int i = 3; i <=max; i+=2)
    //{
    //    if (tb[(i - 3) / 2]) printf("%d ", i);
    //}

    delete[] tb;

//    goto start;

    return 0;
}
