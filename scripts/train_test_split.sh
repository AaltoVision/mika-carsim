#!/bin/bash

mkdir -p train_set
mkdir -p test_set

total_count=`find dataset/ | grep Camera_ | wc -l`

train_count=`echo "$total_count*0.9" | bc | cut -d "." -f 1`
test_count=`echo "$total_count*0.1" | bc | cut -d "." -f 1`

echo "$train_count training examples, $test_count test examples."

find dataset/ | grep Camera_ | shuf | head -n $train_count | xargs -I{} mv {} train_set/

find dataset/ | grep Camera_ | xargs -I{} mv {} test_set/

echo "Train-test split done"

