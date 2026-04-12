# File này để test inference của model LaMa đã được convert sang ONNX, dùng thư viện onnxruntime
# Giả sử đã có file 
    # "lama_fp32.onnx"
    # "image.png" (ảnh gốc)
    # "mask.png" (mask vùng cần inpaint)

import onnxruntime as ort
import numpy as np
import cv2

# load model 
session = ort.InferenceSession("lama_fp32.onnx")

# load image
img = cv2.imread("image.png")
img = cv2.resize(img, (512, 512))
img = img[:, :, ::-1]  # BGR → RGB
img = img.astype(np.float32) / 255.0

# load mask
mask = cv2.imread("mask.png", 0)
mask = cv2.resize(mask, (512, 512))
mask = (mask > 0).astype(np.float32)

# convert to tensor 
img = np.transpose(img, (2, 0, 1))  # (H,W,3) → (3,H,W)
mask = np.expand_dims(mask, 0) # (H,W) → (1,H,W)

img = np.expand_dims(img, 0)   # (3,H,W) → (1,3,H,W)
mask = np.expand_dims(mask, 0) # (1,H,W) → (1,1,H,W)

# inference
outputs = session.run( # khúc này mới thật sự là inference (run cái InferenceSession("lama_fp32.onnx"))
    None,
    {
        "image": img,
        "mask": mask
    }
)

output = outputs[0][0]  # (3,H,W)

# convert back
output = np.transpose(output, (1, 2, 0))
output = (output * 255).astype(np.uint8)

cv2.imwrite("output.png", output)
print("DONE")