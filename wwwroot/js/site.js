document.addEventListener("DOMContentLoaded", function () {

    // ================= 1. XỬ LÝ SIDEBAR (KHỚP VỚI LAYOUT CỦA BẠN) =================
    // Chọn tất cả các phần tử là mục trong sidebar (bao gồm cả link và nút dropdown)
    const sidebarItems = document.querySelectorAll(".sidebar-nav .sidebar-item");

    sidebarItems.forEach(item => {
        item.addEventListener("click", function (e) {
            // Kiểm tra xem nút được bấm có phải là nút mở menu con không?
            // (Trong Layout của bạn, nút mở menu có class "dropdown-toggle")
            const isDropdownToggle = this.classList.contains("dropdown-toggle");

            if (isDropdownToggle) {
                // --- TRƯỜNG HỢP 1: LÀ NÚT MENU CON (Test Suites, Test Cases...) ---
                e.preventDefault(); // Ngăn không cho load lại trang

                // Tìm thẻ cha (.sidebar-dropdown)
                const parent = this.closest(".sidebar-dropdown");

                // (Tùy chọn) Đóng các menu khác đang mở
                document.querySelectorAll(".sidebar-dropdown.open").forEach(d => {
                    if (d !== parent) d.classList.remove("open");
                });

                // Mở/Đóng menu hiện tại
                if (parent) {
                    parent.classList.toggle("open");
                }
            }
            else {
                // --- TRƯỜNG HỢP 2: LÀ LINK THƯỜNG (Dashboard, Projects...) ---
                // Không làm gì cả -> Để trình duyệt tự chuyển trang theo href
            }
        });
    });


    // ================= 2. XỬ LÝ DARK MODE =================
    const themeToggleBtn = document.getElementById('theme-toggle');
    const themeIcon = document.getElementById('theme-icon');
    const htmlElement = document.documentElement;

    const savedTheme = localStorage.getItem('theme');
    if (savedTheme === 'dark') {
        htmlElement.setAttribute('data-theme', 'dark');
        if (themeIcon) themeIcon.textContent = '☀️';
    }

    if (themeToggleBtn) {
        themeToggleBtn.addEventListener('click', () => {
            const currentTheme = htmlElement.getAttribute('data-theme');
            if (currentTheme === 'dark') {
                htmlElement.removeAttribute('data-theme');
                localStorage.setItem('theme', 'light');
                if (themeIcon) themeIcon.textContent = '🌙';
            } else {
                htmlElement.setAttribute('data-theme', 'dark');
                localStorage.setItem('theme', 'dark');
                if (themeIcon) themeIcon.textContent = '☀️';
            }
        });
    }


    // ================= 3. XỬ LÝ USER MENU (Góc trên phải) =================
    const userTrigger = document.getElementById("userTrigger"); // Lưu ý: Trong Layout bạn chưa đặt ID này cho tên user
    // Nếu trong Layout bạn chưa có id="userTrigger", đoạn này có thể chưa chạy.
    // Bạn nên bao quanh tên user bằng thẻ có id="userTrigger" nếu muốn dropdown hoạt động.

    // Tuy nhiên, layout bạn gửi đang dùng các nút rời (Darkmode, Tên, Quản lý, Logout) 
    // nên có thể không cần dropdown user menu phức tạp.


    // ================= 4. XỬ LÝ TEST STEPS (Trang Create/Edit Test Case) =================
    const stepContainer = document.getElementById('steps-container');
    const btnAddStep = document.getElementById('btnAddStep');

    if (stepContainer && btnAddStep) {
        let stepCounter = parseInt(stepContainer.getAttribute('data-step-count')) || 0;

        // Thêm Step
        btnAddStep.addEventListener('click', function () {
            const noStepsMsg = document.getElementById('no-steps-msg');
            if (noStepsMsg) noStepsMsg.style.display = 'none';

            stepCounter++;
            const newStep = document.createElement('div');
            newStep.className = 'step-item';
            newStep.dataset.step = stepCounter;

            newStep.innerHTML = `
                <div class="step-header">
                    <span class="step-badge">Step ${stepCounter}</span>
                    <button type="button" class="btn-close-step" title="Xóa step này">×</button>
                </div>
                <div class="form-grid">
                    <div class="form-group">
                        <label class="form-label">Hành động (Action)</label>
                        <textarea name="Actions[${stepCounter - 1}]" class="form-input" rows="2" placeholder="Nhập hành động..." required></textarea>
                    </div>
                    <div class="form-group">
                        <label class="form-label">Kết quả mong đợi (Expected)</label>
                        <textarea name="ExpectedResults[${stepCounter - 1}]" class="form-input" rows="2" placeholder="Nhập kết quả..."></textarea>
                    </div>
                </div>
            `;
            stepContainer.appendChild(newStep);
        });

        // Xóa Step
        stepContainer.addEventListener('click', function (e) {
            if (e.target && e.target.classList.contains('btn-close-step')) {
                e.preventDefault();
                const stepItem = e.target.closest('.step-item');
                if (stepItem) {
                    stepItem.remove();
                    reIndexSteps();
                }
            }
        });

        // Đánh số lại
        function reIndexSteps() {
            const steps = stepContainer.querySelectorAll('.step-item');
            steps.forEach((step, index) => {
                const newIndex = index + 1;
                step.dataset.step = newIndex;
                const badge = step.querySelector('.step-badge');
                if (badge) badge.textContent = `Step ${newIndex}`;

                const action = step.querySelector('textarea[name^="Actions"]');
                const expected = step.querySelector('textarea[name^="ExpectedResults"]');

                if (action) action.name = `Actions[${index}]`;
                if (expected) expected.name = `ExpectedResults[${index}]`;
            });
            stepCounter = steps.length;
            if (stepCounter === 0) {
                if (!document.getElementById('no-steps-msg')) {
                    stepContainer.innerHTML = '<div style="padding: 20px; text-align: center; color: #888; font-style: italic;" id="no-steps-msg">Chưa có bước kiểm thử nào. Bấm "Thêm Step" để bắt đầu.</div>';
                } else {
                    document.getElementById('no-steps-msg').style.display = 'block';
                }
            }
        }
    }


    // ================= 5. XỬ LÝ TRANG RUN TEST (EXECUTION) =================
    const resultSelect = document.getElementById('ResultSelect');
    const btnSubmitExec = document.getElementById('btnSubmit');

    if (resultSelect && btnSubmitExec) {
        function updateButtonState() {
            const value = resultSelect.value;
            btnSubmitExec.style.backgroundColor = '';
            btnSubmitExec.style.borderColor = '';
            let color = '', text = '', textColor = 'white';

            switch (value) {
                case '1': color = '#16a34a'; text = '✔ LƯU - PASS'; break;
                case '2': color = '#dc2626'; text = '✖ LƯU - FAIL'; break;
                case '4': color = '#64748b'; text = '⏭ LƯU - SKIPPED'; break;
                default: color = '#f59e0b'; text = '⚠ LƯU - PENDING'; textColor = '#000';
            }
            btnSubmitExec.style.backgroundColor = color;
            btnSubmitExec.style.borderColor = color;
            btnSubmitExec.style.color = textColor;
            btnSubmitExec.textContent = text;
        }
        resultSelect.addEventListener('change', updateButtonState);
        updateButtonState();
    }
    const container = document.querySelector('.container');
    const registerBtn = document.querySelector('.register-btn');
    const loginBtn = document.querySelector('.login-btn');

    if (registerBtn && loginBtn && container) {
        registerBtn.addEventListener('click', () => {
            container.classList.add('active');
        });

        loginBtn.addEventListener('click', () => {
            container.classList.remove('active');
        });
    }
    // ... code cũ (Sidebar, Login Slider...) giữ nguyên ...

    // ================= 7. XỬ LÝ PASSWORD STRENGTH (MỚI THÊM) =================
    const passwordInput = document.getElementById('registerPassword');
    const toggleIcon = document.getElementById('toggleRegisterPassword');
    const strengthBar = document.getElementById('strengthBar');
    const feedback = document.getElementById('feedback');

    // Chỉ chạy nếu đang ở trang có form đăng ký
    if (passwordInput && strengthBar) {

        // Các phần tử hiển thị yêu cầu
        const requirements = {
            length: document.getElementById('length'),
            number: document.getElementById('number'),
            lowercase: document.getElementById('lowercase'),
            uppercase: document.getElementById('uppercase'),
            symbol: document.getElementById('symbol')
        };

        // Regex kiểm tra
        const patterns = {
            number: /\d/,
            lowercase: /[a-z]/,
            uppercase: /[A-Z]/,
            symbol: /[^A-Za-z0-9]/
        };

        // 1. Ẩn/Hiện mật khẩu khi bấm vào icon khóa
        if (toggleIcon) {
            toggleIcon.addEventListener('click', () => {
                const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
                passwordInput.setAttribute('type', type);

                // Đổi icon Boxicons (Khóa đóng <-> Khóa mở)
                toggleIcon.classList.toggle('bxs-lock-alt');
                toggleIcon.classList.toggle('bxs-lock-open-alt');
            });
        }

        // 2. Kiểm tra độ mạnh khi nhập liệu
        passwordInput.addEventListener('input', function () {
            const val = passwordInput.value;
            let score = 0;

            // --- Kiểm tra từng điều kiện ---
            // Độ dài >= 8
            if (val.length >= 8) { setValid('length', true); score++; }
            else { setValid('length', false); }

            // Chứa số
            if (patterns.number.test(val)) { setValid('number', true); score++; }
            else { setValid('number', false); }

            // Chữ thường
            if (patterns.lowercase.test(val)) { setValid('lowercase', true); score++; }
            else { setValid('lowercase', false); }

            // Chữ hoa
            if (patterns.uppercase.test(val)) { setValid('uppercase', true); score++; }
            else { setValid('uppercase', false); }

            // Ký tự đặc biệt
            if (patterns.symbol.test(val)) { setValid('symbol', true); score++; }
            else { setValid('symbol', false); }

            // --- Cập nhật giao diện thanh đo ---
            updateMeter(score, val.length);
        });

        // Hàm phụ: Đổi màu icon tích xanh/đỏ
        function setValid(id, isValid) {
            const el = requirements[id];
            if (!el) return;
            const icon = el.querySelector('i');

            if (isValid) {
                el.classList.add('valid');
                icon.className = 'bx bxs-check-circle'; // Icon tích xanh đậm
            } else {
                el.classList.remove('valid');
                icon.className = 'bx bx-x-circle'; // Icon X tròn
            }
        }

        // Hàm phụ: Cập nhật thanh màu
        function updateMeter(score, length) {
            strengthBar.className = 'strength-meter'; // Reset class
            feedback.style.color = '#888';

            if (length === 0) {
                feedback.textContent = '';
                return;
            }

            if (score <= 2) {
                strengthBar.classList.add('weak');
                feedback.textContent = 'Yếu';
                feedback.style.color = '#e74c3c';
            } else if (score <= 4) {
                strengthBar.classList.add('medium');
                feedback.textContent = 'Trung bình';
                feedback.style.color = '#f1c40f';
            } else {
                strengthBar.classList.add('strong');
                feedback.textContent = 'Mạnh';
                feedback.style.color = '#2ecc71';
            }
        }
    }
});