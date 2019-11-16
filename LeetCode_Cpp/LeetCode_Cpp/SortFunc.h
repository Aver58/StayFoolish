#include "Global.h"

/// ������������ֵ
void Swap(int array[], int i, int j)
{
	int temp = array[i];
	array[i] = array[j];
	array[j] = temp;
}

//��������Ϊ�βε��ú���ʱ����Ҫ��ͬ���鳤��һ�𴫽�����
//��������Ϊ�����������ݹ����У����˻���ָ�롣��Ҳ��Ϊʲôָ����Ϊ��������ʱ������Ҫһ�����ȡ�


// 1. ð������ (���մ�С�����˳������) Ч�ʲ��ߣ��ʺ�С��������
// ƽ��ʱ�临�Ӷȣ�O(n^2)
// �ռ临�Ӷȣ�O(1)
// �ȶ��ԣ��ȶ�

// �������������ȶԴ�С������Ƶ�����ȥ
void BubbleSort(int nums[],int len)
{
	// ���������ѡ��Ԫ��
	for (int i = 0; i < len; i++)
	{
		// �ڲ���������ѡ��Ԫ�رȶԴ�С
		for (int j = i + 1; j < len; j++)
		{
			if (nums[j] < nums[i])
			{
				Swap(nums, i, j);
			}
		}
	}
}

// 2. �������� (���մ�С�����˳������)
// ƽ��ʱ�临�Ӷȣ�O(n^2)
// �ռ临�Ӷȣ�O(1)
// �ȶ��ԣ��ȶ�

// ���α�����ֱ�Ӳ��뵽ָ��λ��
void InsertSort(int nums[], int len)
{
	// ���������ѡ��Ԫ��
	for (int i = 0; i < len; i++)
	{
		int target = nums[i];
		int j = i - 1;
		while ((j >= 0) && target < nums[j])
		{
			nums[j + 1] = nums[j];
			j--;
		}
		nums[j + 1] = target;
	}
}

// 3. ѡ������
// ƽ��ʱ�临�Ӷȣ�O(n^2)
// �ռ临�Ӷȣ�O(1)
// �ȶ��ԣ����ȶ�

//��������ѡ����С�������ŵ������������ĩ��
void SelectionSort(int nums[], int len)
{
	for (int i = 0; i < len; i++)
	{
		int min = i;
		for (int j = i + 1; j < len; j++)
		{
			if (nums[j] < nums[min])
			{
				min = j;
			}
		}
		Swap(nums, i, min);
	}
}

// 4. ��������
// ��������Quicksort���Ƕ�ð�������һ�ָĽ���
// ���η�����һ����׼ֵ��Ȼ��˫ָ�������
// ��С���Ƶ���׼ֵ����ߣ�����ұߣ�Ȼ��ݹ��׼ֵ��ߺ��ұ�
// ƽ��ʱ�临�Ӷȣ�O(Logn)
// �ռ临�Ӷȣ�O(1)
// �ȶ��ԣ����ȶ�
int partition(int nums[],int left,int right)
{

	int pivot = nums[right];//ѡ�����һ��Ԫ����Ϊ�Ƚ�Ԫ��
	// ˫ָ�룬iΪ��ָ�룬jΪ��ָ��
	int j = left;
	for (int i = left; i < right - 1; i++) // ע��right - 1,����Խ��
	{
		// ���ұ��ұȻ�׼ֵС��
		// ������ұȻ�׼ֵ���
		if (nums[i] <= pivot)
			Swap(nums, i, j++);
	}
	// ��׼ֵ�ŵ��м�
	Swap(nums, j, right);
	return j;
}

void QuickSort(int nums[], int left, int right)
{
	if (left < right)
	{
		int pivot = partition(nums, left, right);//�ֳ��������룬һ�벻����A[r], һ�벻С��A[r]
		QuickSort(nums, left, pivot - 1);// �ݹ����
		QuickSort(nums, pivot + 1, right);// �ݹ��ұ�
	}
}

//5. �鲢���� 
//���η����ݹ��������У�������������ԭ����
// ƽ��ʱ�临�ӶȾ�ΪO(nlogn)
// �ռ临�Ӷȣ�O(n)
// �ȶ��ԣ��ȶ�
//https://www.cnblogs.com/chengxiao/p/6194356.html
//https://www.runoob.com/w3cnote/merge-sort.html  ����̳�

//���ж�����������a[first...mid]��a[mid...last]�ϲ���
template<typename T>
void _MergeArray(T* nums, int left,int mid, int right, T* temp) {
	// ���Σ� [left,mid) �Ұ�Σ� [mid + 1,right) ���������
	int i = left;
	int j = mid + 1; 
	int t = 0;		//��ʱ����ָ��
	// ˫ָ��ͬʱ�Ƚ����κ��Ұ�Σ�����2��Ƭ��
	while (i <= mid && j <= right)
		temp[t++] = nums[i] <= nums[j] ? nums[i++]: nums[j++];

	//�����ʣ��Ԫ������temp��
	while (i <= mid)
		temp[t++] = nums[i++];
	//��������ʣ��Ԫ������temp��
	while (j <= right)
		temp[t++] = nums[j++];

	// ��temp���������[left,right)λ��
	t = 0;
	while (left <= right)
		nums[left++] = temp[t++];
}

template<typename T>
void MergeSort(T* nums,int left, int right, T* temp) {
	if (left < right)
	{
		int mid = left + (right - left) / 2;			//�������int
		MergeSort(nums, left, mid, temp);				//����߽�������
		MergeSort(nums, mid + 1, right, temp);			//���ұ߽�������
		_MergeArray(nums, left, mid, right, temp);		//������õ����ݺϲ�
	}
}

static void SortTest() {
	int head[7] = { 2,3,4,6,8,7,0};
	//vector<int> head1 = vector<int>{ 2,3,4,5,8,7,0};
	int* head2 = head;
	int* temp = new int[20];
	MergeSort(head2,0,6, temp);
	//merge_sort(head,6);
	//Dump(head1, GetArrayLen(head1));
	//int i = 0;
	for (size_t i = 0; i < 7; i++)
	{
		cout << *head2++;
	}
}
