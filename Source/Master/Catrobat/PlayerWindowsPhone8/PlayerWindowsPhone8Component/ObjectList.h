#pragma once

#include <list>

#include "Object.h"

using namespace std;

class ObjectList
{
public:
	ObjectList();
	~ObjectList();

	void addObject(Object *object);
	int Size();
	Object *getObject(int index);
	Object *getObject(string name);

private:
	list<Object*> *m_objects;
};

